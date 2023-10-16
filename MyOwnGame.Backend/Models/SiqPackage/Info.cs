using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="info")]
public class Info { 

    [XmlElement(ElementName="authors")] 
    public Authors Authors { get; set; } 
}
