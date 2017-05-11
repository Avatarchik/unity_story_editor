﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogBox : SDEContainer {
	
	// components of the DialogBox
	public TextArea dialogArea;
	public ConnectionPoint outPoint;
	
	public GUIStyle textAreaButtonStyle;
	
	public DialogBox() {}
	public void Init(SDEContainer parent, string text) {
		base.Init(parent);
		Init(text);
	}
	
	public void Init(Node parentNode, string text) {
		base.Init(parentNode);
		Init(text);
	}
	
	private void Init(string text) {
		this.dialogArea = ScriptableObject.CreateInstance<TextArea>();
		this.dialogArea.Init(this, text, NodeManager.NODE_WIDTH);
		this.dialogArea.OnDeselect = UpdateInterrupts;
		
		// make the outpoint a child of the dialogArea, so it's bound to that field.
		this.outPoint = ScriptableObject.CreateInstance<ConnectionPoint>();
		this.outPoint.Init(this.dialogArea, ConnectionPointType.Out);
		
		// set the button styles
		this.textAreaButtonStyle = TextAreaManager.textAreaButtonStyle;
	}
	
	public override void Draw() {
		Rect refRect;
		if (parentNode != null) {
			refRect = parentNode.rect;
		} else {
			refRect = parent.rect;
		}
		
		// update container position
		rect.x = refRect.x;
		rect.y = refRect.y + refRect.height;
		
		// draw children
		dialogArea.Draw();
		outPoint.Draw();
		if (child != null) {
			child.Draw();
		}
		
		// update container size
		rect.width = dialogArea.clickRect.width + outPoint.rect.width;
		rect.height = dialogArea.clickRect.height;
		
		// draw remove button
		// don't draw the remove button if its the only child of a Node
		if (child != null || parent != null) {
			if (GUI.Button(new Rect(rect.x-11, rect.y + rect.height/2 - 6, 12, 12), "-", textAreaButtonStyle)) {
				Remove();
			}
		}
	}
	
	public override void ProcessEvent(Event e) {
		// process children first
		if (child != null) {
			child.ProcessEvent(e);
		}
		
		// process component events
		dialogArea.ProcessEvent(e);
		outPoint.ProcessEvent(e);
		
		switch(e.type) {
		case EventType.MouseDown:
			// check for context menu
			if (e.button == 1 && rect.Contains(e.mousePosition)) {
				ProcessContextMenu();
				e.Use();
			}
			break;
			
		case EventType.KeyDown:
			// check for Tab & Shift+Tab cycling
						
			if (e.keyCode == KeyCode.Tab && dialogArea.Selected) {
				if (e.shift) {
					CycleFocusUp();
				} else {
					CycleFocusDown();
				}
				e.Use();
			}
			break;
		}
	}
	
	/*
	  ProcessContextMenu() creates and hooks up the context menu attached to this Node.
	*/
	private void ProcessContextMenu() {
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Remove Dialog"), false, Remove);
		genericMenu.ShowAsContext();
	}
	
	private void CycleFocusDown() {
		Debug.Log("DialogBox: cycling down");
		SDEContainer newFocusedDialogBox = this;
		
		if (child != null) {
			newFocusedDialogBox = child;
		} else if (parent != null) {
			// if at the bottom of the TextArea stack, jump back to the top
			while(newFocusedDialogBox.parent != null) {
				newFocusedDialogBox = newFocusedDialogBox.parent;
			}
		}
		
		// transfer selection state
		dialogArea.Selected = false;
		((DialogBox)newFocusedDialogBox).dialogArea.Selected = true;

		// pass keyboard control
		GUIUtility.keyboardControl = ((DialogBox)newFocusedDialogBox).dialogArea.textID;
	}
	
	private void UpdateInterrupts(SDEComponent textArea) {
		Debug.Log("DialogBox: Updating interrupt options...");
		return;
		
		string text = ((TextArea)textArea).text;
		
		// parse the text for interrupts flags
		List<string> flags = new List<string>();
		// TODO: implement the rest
		
		// find an Interrupt Node that's connected to this
		Node interruptNode = DialogBoxManager.GetInterruptNode(outPoint);
		if (interruptNode == null) {
			// create a new Interrupt Node and connect them
			// TODO: implement this
		}
		
		// update the Interrupt Node
		// TODO: implement this
	}
	
	private void CycleFocusUp() {
		Debug.Log("DialogBox: cycling up");
		SDEContainer newFocusedDialogBox = this;
		
		if (parent != null) {
			newFocusedDialogBox = parent;
		} else if (child != null) {
			// if at the top of the DialogBox stack, jump back to the bottom
			while (newFocusedDialogBox.child != null) {
				newFocusedDialogBox = newFocusedDialogBox.child;
			}
		}
		
		// transfer Selection state
		dialogArea.Selected = false;
		((DialogBox)newFocusedDialogBox).dialogArea.Selected = true;
		
		// pass keyboard control
		GUIUtility.keyboardControl = ((DialogBox)newFocusedDialogBox).dialogArea.textID;
	}
	
	/*
	  Remove() is a wrapper for the DialogBoxManager's RemoveDialogBox function, so it
	  can be passed to the ContextMenu's menu function argument.
	*/
	private void Remove() {
		DialogBoxManager.RemoveDialogBox(this);
	}
}
