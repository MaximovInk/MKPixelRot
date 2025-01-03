namespace MaximovInk
{
    public class MKPixelRotContainer : MonoBehaviourSingletonAuto<MKPixelRotContainer>
    {


        public void Clear()
        {
            MKUtils.DestroyAllChildren(transform);
        }

    }
}
