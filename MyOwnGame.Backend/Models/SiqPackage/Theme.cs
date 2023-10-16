using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="theme")]
public class Theme { 

    [XmlElement(ElementName="questions")] 
    public Questions Questions { get; set; } 

    [XmlAttribute(AttributeName="name")] 
    public string Name { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}