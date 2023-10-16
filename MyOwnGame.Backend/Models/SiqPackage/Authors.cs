using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="authors")]
public class Authors { 

    [XmlElement(ElementName="author")] 
    public string Author { get; set; } 
}