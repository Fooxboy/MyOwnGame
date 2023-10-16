using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="package")]
public class Package { 

    [XmlElement(ElementName="tags")] 
    public Tags Tags { get; set; } 

    [XmlElement(ElementName="info")] 
    public Info Info { get; set; } 

    [XmlElement(ElementName="rounds")] 
    public Rounds Rounds { get; set; } 

    [XmlAttribute(AttributeName="name")] 
    public string Name { get; set; } 

    [XmlAttribute(AttributeName="version")] 
    public int Version { get; set; } 

    [XmlAttribute(AttributeName="id")] 
    public string Id { get; set; } 

    [XmlAttribute(AttributeName="date")] 
    public string Date { get; set; } 

    [XmlAttribute(AttributeName="difficulty")] 
    public int Difficulty { get; set; } 

    [XmlAttribute(AttributeName="xmlns")] 
    public string Xmlns { get; set; } 
    
    [XmlAttribute(AttributeName="logo")] 
    public string Logo { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}
