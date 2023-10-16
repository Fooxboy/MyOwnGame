using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="rounds")]
public class Rounds { 

    [XmlElement(ElementName="round")] 
    public List<Round> Round { get; set; } 
}