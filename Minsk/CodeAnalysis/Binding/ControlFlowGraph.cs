using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraph
    {
        private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        public sealed class BasicBlock
        {
            public BasicBlock()
            {
            }

            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
            public List<BasicBlockBranch> Incoming { get; } = new List<BasicBlockBranch>();
            public List<BasicBlockBranch> Outgoing { get; } = new List<BasicBlockBranch>();
            public bool IsStart { get; }
            public bool IsEnd { get; }

            public override string ToString()
            {
                if (IsStart)
                {
                    return "<Start>";
                }

                if (IsEnd)
                {
                    return "<End>";
                }

                using (var stringWriter = new StringWriter())
                {
                    foreach (var statement in Statements)
                    {
                        statement.WriteTo(stringWriter);
                    }
                    return stringWriter.ToString();
                }
            }
        }

        public sealed class BasicBlockBranch
        {
            public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression Condition { get; }

            public override string ToString()
            {
                if (Condition == null)
                {
                    return string.Empty;
                }

                return Condition.ToString();
            }
        }

        public sealed class BasicBlockBuilder
        {
            private readonly List<BoundStatement> _statements = new List<BoundStatement>();
            private readonly List<BasicBlock> _blocks = new List<BasicBlock>();

            public List<BasicBlock> Build(BoundBlockStatement block)
            {
                foreach (var statement in block.Statements)
                {
                    switch (statement.Kind)
                    {
                        case BoundNodeKind.ConditionalGotoStatement:
                        case BoundNodeKind.GotoStatement:
                        case BoundNodeKind.ReturnStatement:
                            _statements.Add(statement);
                            StartBlock();
                            break;
                        case BoundNodeKind.LabelStatement:
                            StartBlock();
                            _statements.Add(statement);
                            break;
                        case BoundNodeKind.ExpressionStatement:
                        case BoundNodeKind.VariableDeclaration:
                            _statements.Add(statement);
                            break;
                        default:
                            throw new Exception($"Unexcepted statement: {statement.Kind}.");
                    }
                }

                EndBlock();

                return _blocks.ToList();
            }

            private void EndBlock()
            {
                if (_statements.Any())
                {
                    var block = new BasicBlock();
                    block.Statements.AddRange(_statements);
                    _blocks.Add(block);
                    _statements.Clear();
                }
            }

            private void StartBlock()
            {
                EndBlock();
            }
        }

        public sealed class GraphBuilder
        {
            private Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            private Dictionary<BoundLabel, BasicBlock> _blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
            private List<BasicBlockBranch> _branches = new List<BasicBlockBranch>();
            private readonly BasicBlock _start = new BasicBlock(isStart: true);
            private readonly BasicBlock _end = new BasicBlock(isStart: false);

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                if (!blocks.Any())
                {
                    Connect(_start, _end);
                }
                else
                {
                    Connect(_start, blocks.First());
                }

                foreach (var block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        _blockFromStatement.Add(statement, block);
                        if (statement is BoundLabelStatement labelStatement)
                        {
                            _blockFromLabel.Add(labelStatement.Label, block);
                        }
                    }
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    var current = blocks[i];
                    var next = i == blocks.Count - 1 ? _end : blocks[i + 1];

                    foreach (var statement in current.Statements)
                    {
                        var isLastStatementInBlock = statement == current.Statements.Last();
                        Walk(statement, current, next, isLastStatementInBlock);
                    }
                }

            ScanAgain:
                foreach (var block in blocks)
                {
                    if (!block.Incoming.Any())
                    {
                        RemoveBlock(blocks, block);
                        goto ScanAgain;
                    }
                }

                blocks.Insert(0, _start);
                blocks.Add(_end);

                return new ControlFlowGraph(_start, _end, blocks, _branches);
            }

            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                blocks.Remove(block);
                foreach (var branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    _branches.Remove(branch);
                }
                foreach (var branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    _branches.Remove(branch);
                }
            }

            private void Walk(BoundStatement statement, BasicBlock current, BasicBlock next, bool isLastStatementInBlock)
            {
                switch (statement.Kind)
                {
                    case BoundNodeKind.ConditionalGotoStatement:
                        var conditionalGotoStatement = (BoundConditionalGotoStatement)statement;
                        var thenBlock = _blockFromLabel[conditionalGotoStatement.Label];
                        var elseBlock = next;
                        var negatedCondition = Negate(conditionalGotoStatement.Condition);
                        var thenCondition = conditionalGotoStatement.JumpIfTrue ? conditionalGotoStatement.Condition : negatedCondition;
                        var elseConditon = conditionalGotoStatement.JumpIfTrue ?
                            negatedCondition :
                            conditionalGotoStatement.Condition;
                        Connect(current, thenBlock, thenCondition);
                        Connect(current, elseBlock, elseConditon);
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gotoStatement = (BoundGotoStatement)statement;
                        var toBlock = _blockFromLabel[gotoStatement.Label];
                        Connect(current, toBlock);
                        break;
                    case BoundNodeKind.ReturnStatement:
                        Connect(current, _end);
                        break;
                    case BoundNodeKind.LabelStatement:
                    case BoundNodeKind.ExpressionStatement:
                    case BoundNodeKind.VariableDeclaration:
                        if (isLastStatementInBlock)
                        {
                            Connect(current, next);
                        }
                        break;

                    default:
                        throw new Exception($"Unexcepted statement: {statement.Kind}.");
                }
            }

            private BoundExpression Negate(BoundExpression condition)
            {
                if (condition is BoundLiteralExpression literal)
                {
                    var value = (bool)literal.Value;
                    return new BoundLiteralExpression(!value);
                }

                var unaryOperator = BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool);
                return new BoundUnaryExpression(unaryOperator, condition);
            }

            private void Connect(BasicBlock from, BasicBlock to, BoundExpression condition = null)
            {
                if (condition is BoundLiteralExpression l)
                {
                    var value = (bool)l.Value;
                    if (value)
                    {
                        condition = null;
                    }
                    else
                    {
                        return;
                    }
                }
                var branch = new BasicBlockBranch(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                _branches.Add(branch);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            string Quote(string text) => $"\"{text.Replace("\"", "\\\"")}\"";

            writer.WriteLine("digraph G {");

            var blockIds = new Dictionary<BasicBlock, string>();

            for (int i = 0; i < Blocks.Count; i++)
            {
                var id = $"N{i}";
                blockIds.Add(Blocks[i], id);
            }

            foreach (var block in Blocks)
            {
                var id = blockIds[block];
                var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
                writer.WriteLine($"    {id} [label = {label}, shape = box]");
            }

            foreach (var branch in Branches)
            {
                var fromId = blockIds[branch.From];
                var toId = blockIds[branch.To];
                var label = Quote(branch.ToString());
                writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }

        public static ControlFlowGraph Create(BoundBlockStatement body)
        {
            var basicBlockBuilder = new BasicBlockBuilder();
            var blocks = basicBlockBuilder.Build(body);

            var graphBuilder = new GraphBuilder();
            return graphBuilder.Build(blocks);
        }

        public static bool AllPathsReturn(BoundBlockStatement body)
        {
            var graph = Create(body);
            foreach (var branch in graph.End.Incoming)
            {
                var lastStatement = branch.From.Statements.Last();

                if (lastStatement.Kind != BoundNodeKind.ReturnStatement)
                {
                    return false;
                }
            }

            return true;
        }
    }
}