using System.Collections.Generic;

public class RoomData
{
    public List<MaskData> AllowedMasks;

    public bool CanEnter(MaskData mask)
    {
        return mask != null && AllowedMasks.Contains(mask);
    }
}
