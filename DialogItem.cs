namespace GMTK2024;

internal enum PersonName
{
    Hans,
    Private
};

internal class DialogItem
{
    public PersonName person;
    public string text;

    public DialogItem(PersonName person, string text)
    {
        this.person = person;
        this.text = text;
    }
}
