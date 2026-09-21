using UnityEngine;

public class BondProgressManager : MonoBehaviour
{
    private int brokenBonds = 0;
    private const int requiredBonds = 2;

    public bool BothBondsAreBroken
    {
        get
        {
            return brokenBonds >= requiredBonds;
        }
    }

    public void BondBroken()
    {
        brokenBonds++;

        Debug.Log("H-Cl bonds broken: " + brokenBonds + "/" + requiredBonds);

        if (BothBondsAreBroken)
        {
            BothBondsBroken();
        }
    }

    private void BothBondsBroken()
    {
        Debug.Log("Both H-Cl bonds successfully broken!");
        Debug.Log("Next objective: Bring the two hydrogen atoms together to form H2.");
    }
}