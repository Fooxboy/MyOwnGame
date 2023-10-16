using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="tags")]
public class Tags { 

    [XmlElement(ElementName="tag")] 
    public List<string> Tag { get; set; } 
}