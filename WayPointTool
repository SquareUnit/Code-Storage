using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;

// Test linktolast

public class Node
{
    public static int nodeCount;
    public int data;
    public Node next;
    public Node previous;
    public static string linkedListData;

    public Node(int i, Node previousNode)
    {
        nodeCount++;
        data = i;
        next = null;
        this.previous = previousNode;
    }

    public void AddToEnd(int data)
    {
        if (next == null)
        {
            next = new Node(data, this);
        }
        else
        {
            next.AddToEnd(data);
        }
    }

    public void AddSorted(int data)
    {
        if (next == null)
        {
            // Potential source of error. My fuck up the ordering of the first and second node. Check condition to fix mistake
            next = new Node(data, this);
        }
        else if (data < next.data)
        {
            // If my data val is smaller then this node next data value, my next become this nodes next, then this node next become me.
            Node temp = new Node(data, this);
            next.previous = temp;
            temp.next = this.next;
            this.next = temp;
        }
        else
        {
            next.AddSorted(data);
        }
    }

    public void RemoveLast()
    {
        if (this.next.next != null)
        {
            next.RemoveLast();
        }
        else
        {
            nodeCount--;
            this.next = null;
        }
    }

    public void LinkLastToHead(Node headNode)
    {
        if(next == null)
        {
            this.next = headNode;
        }
        else
        {
            next.LinkLastToHead(headNode);
        }
    }

    public void Print()
    {
        if (previous == null)
        {
            Debug.Log("My val " + data.ToString() + " and I am the first value of the linked list");
        }
        else
        {
            Debug.Log("My val " + data.ToString() + " Previous node val : " + previous.data.ToString());
        }

        string nodeCurrData = ("|" + data.ToString() + "|-> ");
        linkedListData += nodeCurrData;
        if (next != null)
        {
            next.Print();
        }
        else
        {
            Debug.Log(" Values in linked list " + linkedListData + " Node count is : " + nodeCount.ToString());
            linkedListData = null;
        }
    }
}

public class LinkedList
{
    public Node headNode;

    public LinkedList()
    {
        headNode = null;
    }

    public void AddtoEnd(int data)
    {
        if (headNode == null)
        {
            headNode = new Node(data, null);
        }
        else
        {
            headNode.AddToEnd(data);
        }
    }

    public void AddSorted(int data)
    {
        if (headNode == null)
        {
            headNode = new Node(data, null);
        }
        else if (data < headNode.data)
        {
            AddToBeginning(data);
        }
        else
        {
            headNode.AddSorted(data);
        }
    }

    public void AddToBeginning(int data)
    {
        if (headNode == null)
        {
            headNode = new Node(data, null);
        }
        else
        {
            Node temp = new Node(data, null);
            headNode.previous = temp;
            temp.next = headNode;
            headNode = temp;
        }
    }

    public void RemoveLast()
    {
        if (headNode.next == null)
        {
            headNode = null;
        }
        else if (headNode.next.next != null)
        {
            headNode.next.RemoveLast();
        }
        else
        {
            headNode.next = null;
        }
    }

    public void LinkLastToHead()
    {
        if(headNode != null && headNode.next != null)
        {
            headNode.LinkLastToHead(headNode);
        }
        else
        {
            Debug.Log("Cannot link last node to headnode because. Looping linked list require a minimum of two nodes or headnode might be null");
        }
    }

    public void Print()
    {
        if (headNode != null)
        {
            headNode.Print();
        }
        else
        {
            Debug.Log("This linked list is empty");
        }
    }
}

public class WayPointTool : MonoBehaviour
{
    public LinkedList lkList;
    public List<NodeData> nodeData = new List<NodeData>();
    public bool isLooping;

    private void Start()
    {
        lkList = new LinkedList();
    }

    public void PopulateList()
    {
        for (int i = 0; i < nodeData.Count; i++)
        {
            lkList.AddSorted(nodeData[i].intValue);
        }
        if (isLooping) lkList.LinkLastToHead();
    }

    public void PrintList()
    {
        lkList.Print();
    }

    public void ClearList()
    {
        while(lkList.headNode != null)
        {
            lkList.RemoveLast();
        }
    }

    public struct NodeData
    {
        public string name;
        public int intValue;

        public NodeData(int intValue, string name)
        {
            this.name = name;
            this.intValue = intValue;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WayPointTool))]
public class WayPointToolEditor : Editor
{
    WayPointTool wayPointTool;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        wayPointTool = (WayPointTool)target;
        if(GUILayout.Button("Generate linked list"))
        {
            wayPointTool.PopulateList();
        }
        if (GUILayout.Button("Print list content"))
        {
            wayPointTool.PrintList();
        }
        if (GUILayout.Button("Clear list"))
        {
            wayPointTool.ClearList();
        }
    }
}
#endif

