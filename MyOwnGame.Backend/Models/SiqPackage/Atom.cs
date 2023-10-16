using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

public class Atom
{
    [XmlAttribute(AttributeName="type")] 
    public string? Type { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}