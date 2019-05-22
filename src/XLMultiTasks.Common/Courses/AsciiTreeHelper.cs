using System.Text;

namespace XLMultiTasks.Courses
{
    public class AsciiTreeHelper
    {
        public const string _cross = " ├─";
        public const string _corner = " └─";
        public const string _vertical = " │ ";
        public const string _space = "   ";

        public AsciiTreeHelper()
        {
            PrintAllDeep();
        }

        public void PrintAllDeep()
        {
            MaxPrintDeep = -1;
        }

        public int MaxPrintDeep { get; set; }

        public void ProcessNode(Node node, string indent, StringBuilder sb, int currentDeep)
        {
            sb.AppendLine(node.Name);
            var numberOfChildren = node.Children.Count;
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = node.Children[i];
                var isLast = (i == (numberOfChildren - 1));
                ProcessChildNode(child, indent, isLast, sb, currentDeep + 1);
            }
        }

        private void ProcessChildNode(Node node, string indent, bool isLast, StringBuilder sb, int currentDeep)
        {
            if (MaxPrintDeep != -1 && currentDeep > MaxPrintDeep)
            {
                return;
            }
            // Print the provided pipes/spaces indent
            sb.Append(indent);

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                sb.Append(_corner);
                indent += _space;
            }
            else
            {
                sb.Append(_cross);
                indent += _vertical;
            }

            ProcessNode(node, indent, sb, currentDeep);
        }
    }
}