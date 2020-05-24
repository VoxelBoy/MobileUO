using System.IO;
using System.Text;
using UnityEngine;

public class ConsoleRedirect : TextWriter
{
    private readonly StringBuilder buffer = new StringBuilder();

    public override void Flush()
    {
        string str;

        lock(this)
        {
            str = buffer.ToString();
            buffer.Length = 0;
        }

        Debug.Log(str);
    }

    private void FlushNoLock()
    {
        string str = buffer.ToString();
        buffer.Length = 0;
        Debug.Log(str);
    }

    public override void Write(string value)
    {
        if (value != null)
        {

            var len = value.Length;
            if (len > 0)
            {
                lock(this)
                {
                    var lastChar = value [len - 1];
                    if (lastChar == '\n')
                    {
                        if(len>1)
                            buffer.Append(value,0,len-1);
                        FlushNoLock();
                    }
                    else
                    {
                        buffer.Append(value,0,len);
                    }
                }
            }
        }
    }

    public override void Write(char value)
    {
        lock(this)
        {
            if (value == '\n')
            {
                FlushNoLock();
            }
            else
            {
                buffer.Append(value);
            }
        }
    }

    public override void Write(char[] value, int index, int count)
    {
        if(count>0)
        {
            lock(this)
            {
                var lastChar = value [index + count - 1];
                if (lastChar == '\n')
                {
                    if(count>1)
                        buffer.Append(value,index,count-1);
                    FlushNoLock();
                }
                else
                {
                    buffer.Append(value,index,count);
                }
            }
        }
    }

    public override Encoding Encoding
    {
        get { return Encoding.Default; }
    }

    //[RuntimeInitializeOnLoadMethod]
    public static void Redirect()
    {
        System.Console.SetOut(new ConsoleRedirect());
    }
}