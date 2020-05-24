function main()
{
    var name = "something"
    var otherName = "something else"
    print("What is your name?")
    otherName = name = input()
    print("Hello " + name + "!")
    print(otherName)
}