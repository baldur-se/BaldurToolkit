using System;
using System.Text;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public static class TaskTreeInspector
    {
        public delegate void NodeInspector(ITask node, ITask parent, int level);

        public static void IterateTree(ITask tree, NodeInspector inspectorCallback, int maxDepth = -1)
        {
            IterateTree(tree, null, 0, maxDepth, inspectorCallback);
        }

        public static string DumpTreeState(ITask tree, int maxDepth = -1, int indentation = 2)
        {
            var output = new StringBuilder();
            IterateTree(
                tree,
                (node, parent, level) =>
                {
                    output.Append(' ', indentation * level);
                    output.Append(node);
                    output.Append(": ");
                    output.Append(node.Status);

                    if (node is BaseTask task && task.ExecutionCounter > 0)
                    {
                        output.Append(" *");
                        output.Append(task.ExecutionCounter);
                    }

                    output.Append(Environment.NewLine);
                },
                maxDepth);

            return output.ToString();
        }

        private static void IterateTree(ITask node, ITask parent, int level, int maxDepth, NodeInspector inspectorCallback)
        {
            inspectorCallback(node, parent, level);

            if (maxDepth < 0 || level < maxDepth)
            {
                if (node is IDecoratorTask decorator)
                {
                    IterateTree(decorator.BaseTask, node, level + 1, maxDepth, inspectorCallback);
                }

                if (node is ICompositeTask composite)
                {
                    foreach (var child in composite.GetChildren())
                    {
                        IterateTree(child, node, level + 1, maxDepth, inspectorCallback);
                    }
                }
            }
        }
    }
}
