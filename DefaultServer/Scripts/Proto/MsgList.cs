using System.Collections.Generic;

public class MsgList : MsgBase
{
    public MsgEnter[] enterList;
    public MsgList()
    {
        protoName = "MsgList";
    }
}