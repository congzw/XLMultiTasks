using System.Collections.Generic;

namespace XLMultiTasks.Courses
{
    public class Node
    {
        public string Name { get; set; }
        public List<Node> Children { get; } = new List<Node>();
    }
}