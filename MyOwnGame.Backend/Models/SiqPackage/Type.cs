using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="type")]
public class Type { 

    [XmlAttribute(AttributeName="name")] 
    public string Name { get; set; } 

    [XmlElement(ElementName="param")] 
    public List<Param> Param { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}