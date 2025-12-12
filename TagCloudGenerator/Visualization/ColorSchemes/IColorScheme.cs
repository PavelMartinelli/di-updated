using System.Drawing;

namespace TagCloudGenerator;


public interface IColorScheme
{
    Color GetColorForWord(WordTag word, int index, int total);
}