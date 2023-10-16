using System.Xml.Serialization;

namespace MyOwnGame.Backend.Models.SiqPackage;

[XmlRoot(ElementName="questions")]
public class Questions { 

    [XmlElement(ElementName="question")] 
    public List<Question> Question { get; set; } 
}