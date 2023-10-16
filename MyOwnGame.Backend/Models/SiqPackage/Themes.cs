using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="themes")]
public class Themes { 

    [XmlElement(ElementName="theme")] 
    public List<Theme> Theme { get; set; } 
}