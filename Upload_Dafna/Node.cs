using System;
public class Node<T>
{
    private T value;
    private Node<T> next;

    public Node(T value)
    {
        this.value = value;
        this.next = null;
    }

    public Node(T value, Node<T> next)
    {
        this.value = value;
        this.next = next;
    }
    public T GetValue() { return this.value; }
    public void SetValue(T value) { this.value = value; }
    public Node<T> GetNext() { return this.next; }
    public void SetNext(Node<T> next) { this.next = next; }
    public bool HasNext() { return this.next != null; }

    public override string ToString()
    {
        return ""+this.value;
        //string nextValue = this.next != null ? this.next.GetValue().ToString() : "null";
        //return $"{this.value} --> {nextValue}";
    }
}