using Raylib_CsLo;
using System.Numerics;
using System.Security.Cryptography;

namespace GMTK2024;

internal class AvatarFace
{
    public float mouthRotation = 0;
    public float mouthOffset = 0;

    public AnimationState mouthState = new AnimationState();
    public bool playedMouth = true;

    Texture face;
    Texture hat;
    RaylibAnimation mouth;
    Vector2 mouthPivot;

    public AvatarFace(Texture face, Texture hat, RaylibAnimation mouth, Vector2 mouthPivot)
    {
        this.face = face;
        this.hat = hat;
        this.mouth = mouth;
        this.mouthPivot = mouthPivot;
    }

    public static AvatarFace CreateHans()
    {
        return new AvatarFace(Program.hansFace, Program.hansHat, Program.hansMouth, Program.hansMouthPivot);
    }
    public static AvatarFace CreatePrivate()
    {
        return new AvatarFace(Program.privateFace, Program.privateHat, Program.privateMouth, Program.privateMouthPivot);
    }


    public void Update(float dt)
    {
        Program.hansMouth.PlayOnce(dt, ref mouthState, ref playedMouth);
        
        mouthOffset -= 20f * dt * mouthOffset;
        mouthRotation *= 0.9f;
    }

    public void Talk()
    {
        var rng = new Random();
        
        mouthRotation += Utils.RandRange(rng, -1, 1) * (float)Math.PI * 3;
        mouthOffset -= Utils.RandRange(rng, 2, 6);

        playedMouth = false;
    }
    
    public void Draw(Vector2 position, float rotation = 0, float scale = 1, Color? color = null)
    {
        color ??= Raylib.WHITE;

        RlGl.rlPushMatrix();
        {
            RlGl.rlTranslatef(position.X, position.Y, 0);
            RlGl.rlScalef(scale, scale, 0);
            RlGl.rlRotatef(rotation, 0, 0, 1);

            Utils.DrawTextureCentered(face, Vector2.Zero, 0, 1, color.Value);
            mouth.Draw(mouthState,
                new Vector2(0, mouthOffset) + (mouthPivot - mouth.size / 2),
                mouthPivot,
                mouthRotation,
                1,
                color.Value
            );
            Utils.DrawTextureCentered(hat, Vector2.Zero, 0, 1, color.Value);
        }
        RlGl.rlPopMatrix();
    }
}
