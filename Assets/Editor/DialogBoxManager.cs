﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class DialogBoxManager {
	
	public static StoryDialogEditor mainEditor;
	
	public static Node GetInterruptNode(ConnectionPoint point) {
		// there can't be an associated interrupt node if there are no nodes or connections
		if (mainEditor.nodes == null || mainEditor.connections == null) {
			return null;
		}
		
		Node node;
		for (int i = 0; i < mainEditor.nodes.Count; i++) {
			node = mainEditor.nodes[i];
			if (node.nodeType == NodeType.Interrupt) {
				// look for a connection that matches the node with
				// the given connection point
				for (int j = 0; j < mainEditor.connections.Count; j++) {
					if (node.inPoint == mainEditor.connections[j].inPoint) {
						if (mainEditor.connections[j].outPoint == point) {
							return node;
						}
					}
				}
				
			}
		}
		
		// couldn't find a connected Interrupt Node
		return null;
	}
	
	/*
	  RemoveDialogBox() removes the given DialogBox from its Node, and stitches
	  the parent and child components together, removing any old connections.
	*/
	public static void RemoveDialogBox(DialogBox dialogBox) {
		// the parent, child, text area itself, and mainEditor all get modified
		if (dialogBox.parent != null) {
			Undo.RecordObject(dialogBox.parent, "removing text area...");	
		}
		if (dialogBox.parentNode != null) {
			Undo.RecordObject(dialogBox.parentNode, "removing text area...");	
		}
		if (dialogBox.child != null) {
			Undo.RecordObject(dialogBox.child, "removing text area...");
		}
		Undo.RecordObject(dialogBox, "removing text area...");
		Undo.RecordObject(mainEditor, "removing ConnectionPoint connections...");
		
		// stitch the parent to the child
		Node parentNode = dialogBox.parentNode;
		SDEContainer parent = dialogBox.parent;
		SDEContainer child = dialogBox.child;
		
		if (parentNode != null) {
			if (child != null) {
				// stitch the Node and the DialogBox child together
				parentNode.childContainer = child;
				
				child.parentNode = parentNode;
				child.parent = null;
			} else {
				// tried to remove last DialogBox of the Node!
				throw new UnityException("Can't remove last DialogBox of a Node!");
			}
		} else {
			if (parent != null && child != null) {
				// switch the parent and child DialogBox together
				parent.child = child;
				child.parent = parent;
			} else if (parent != null && child == null) {
				// removing the last DialogBox of the Node
				parent.child = null;
			} else {
				// something bad happened!
				throw new UnityException("Tried to RemoveDialogBox with erroneous parent/child!");
			}
		}
		
		// remove all associated connections
		List<Connection> connectionsToRemove = ConnectionManager.GetConnections(dialogBox.outPoint);
		for (int i = 0; i < connectionsToRemove.Count; i++) {
			mainEditor.connections.Remove(connectionsToRemove[i]);
		}
		connectionsToRemove = null;
		
		// free the dialogBox itself
		dialogBox = null;
		
		Undo.FlushUndoRecordObjects();
	}
}
