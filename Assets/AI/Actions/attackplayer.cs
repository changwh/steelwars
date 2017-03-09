using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Action;
using RAIN.Core;

[RAINAction]
public class attackplayer : RAINAction
{
    public override void Start(RAIN.Core.AI ai)
    {
        base.Start(ai);
    }

    public override ActionResult Execute(RAIN.Core.AI ai)
    {
        var attackNum = ai.WorkingMemory.GetItem<int>("attackNum");
        var enemy = ai.WorkingMemory.GetItem<GameObject>("playerobj");
        if(enemy!=null)
        {
			ThirdPerson2FirstPerson con= GameObject.Find("GameController").GetComponent<ThirdPerson2FirstPerson>();
            con.blood -= attackNum;
            Debug.Log(con.blood);

        }
        else
        {
            return ActionResult.FAILURE;
        }
        return ActionResult.SUCCESS;
    }

    public override void Stop(RAIN.Core.AI ai)
    {
        base.Stop(ai);
    }
}