using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

namespace Yaapm.DReS.Optional;

public class FileDotEngine : IDotEngine
{    
    public string Run(GraphvizImageType imageType, string dot, string outputFileName)
    {
        using (var writer = new StreamWriter(outputFileName))
        {
            writer.Write(dot);
        }

        return Path.GetFileName(outputFileName);
    }
}
