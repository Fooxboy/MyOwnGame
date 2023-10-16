using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="scenario")]
public class Scenario { 

    [XmlElement(ElementName="atom")] 
    public List<Atom> Atom { get; set; } 
}