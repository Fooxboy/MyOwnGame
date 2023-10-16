using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="question")]
public class Question { 

    [XmlElement(ElementName="scenario")] 
    public Scenario Scenario { get; set; } 

    [XmlElement(ElementName="right")] 
    public Right Right { get; set; } 
    
    [XmlElement(ElementName="wrong")] 
    public Right? Wrong { get; set; } 

    [XmlAttribute(AttributeName="price")] 
    public int Price { get; set; } 

    [XmlText] 
    public string Text { get; set; } 

    [XmlElement(ElementName="type")] 
    public Type? Type { get; set; } 
}