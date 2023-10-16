using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="round")]
public class Round { 

    [XmlElement(ElementName="themes")] 
    public Themes Themes { get; set; } 

    [XmlAttribute(AttributeName="name")] 
    public string Name { get; set; } 

    [XmlText] 
    public string Text { get; set; } 

    [XmlAttribute(AttributeName="type")] 
    public string Type { get; set; } 
}