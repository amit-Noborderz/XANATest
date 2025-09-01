import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayerActivity;
public class CustomUnityPlayerActivity extends UnityPlayerActivity
{
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data)
{
    super.onActivityResult(requestCode, resultCode, data);
    //FilePicker.getInstance().onActivityResult(requestCode, resultCode, data);
}

 @Override
  protected void onCreate(Bundle savedInstanceState) {
    if (mUnityPlayer != null) {
      mUnityPlayer.quit();
      mUnityPlayer = null;
    }
    super.onCreate(savedInstanceState);
}
}
