using Godot;

namespace HextechRunes;

internal static class GodotNodeCompat
{
	internal static void MoveChildSafely(this Node? parent, Node? child, int index)
	{
		if (parent == null || !GodotObject.IsInstanceValid(parent) || child == null || !GodotObject.IsInstanceValid(child))
		{
			return;
		}

		if (child.GetParent() != parent)
		{
			return;
		}

		int childCount = parent.GetChildCount();
		if (childCount <= 0)
		{
			return;
		}

		int clampedIndex = Math.Clamp(index, 0, childCount - 1);
		parent.MoveChild(child, clampedIndex);
	}
}