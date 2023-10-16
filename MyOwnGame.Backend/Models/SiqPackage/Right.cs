using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="right")]
public class Right { 

    [XmlElement(ElementName="answer")] 
    public string Answer { get; set; } 
}