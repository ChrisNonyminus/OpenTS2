using OpenTS2.Audio;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine.Audio;
using OpenTS2.Files;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OpenTS2.Game;
using UnityEngine.UI;
using OpenTS2.Files.Formats.RCOL;
using OpenTS2.Game.Reimpl;
using OpenTS2.Files.Formats.DBPF;
using System.Linq;

namespace OpenTS2.Engine.Tests
{
    public class AsyncTest : MonoBehaviour
    {
        public AudioSource MusicSource;
        public Text ProgressText;
        public Text ObjectsText;
        public RawImage PopupBackgroundImage;
        public RawImage BackgroundImage;

        //public List<string> packageList;
        public bool async = true;
        System.Diagnostics.Stopwatch stopW;
        // Start is called before the first frame update
        void Start()
        {
            var contentManager = ContentManager.Get();
            contentManager.LoadContentStartup();
            PluginSupport.Initialize();
            MusicSource.clip = AudioManager.SplashAudio.Clip;
            MusicSource.Play();
            stopW = new System.Diagnostics.Stopwatch();
            stopW.Start();
            if (async)
                contentManager.LoadGameContentAsync().ContinueWith((task) => { OnFinishLoading(); }, TaskScheduler.FromCurrentSynchronizationContext());
            else
            {
                contentManager.LoadGameContentSync();
                OnFinishLoading();
            }
        }

        void Update()
        {
            var contentManager = ContentManager.Get();
            ProgressText.text = Mathf.Round(contentManager.ContentLoadProgress.Percentage * 100f).ToString() + "%";
        }

        void OnFinishLoading()
        {
            var contentManager = ContentManager.Get();
            var oMgr = ObjectManager.Get();
            oMgr.Initialize();
            stopW.Stop();
            UnityEngine.Debug.Log("Done loading packages!");
            UnityEngine.Debug.Log(contentManager.Provider.ContentEntries.Count + " packages loaded.");
            UnityEngine.Debug.Log("Package loading took " + (stopW.ElapsedTicks * 1000000 / System.Diagnostics.Stopwatch.Frequency) + " microseconds");
            var objectStr = "Object Amount: " + oMgr.Objects.Count + System.Environment.NewLine;
            for(var i=0;i<200;i++)
            {
                if (i >= oMgr.Objects.Count)
                    break;
                var element = oMgr.Objects[(oMgr.Objects.Count-1)-i];
                objectStr += element.Definition.FileName + " (" + "0x"+element.GUID.ToString("X8") + ")" + System.Environment.NewLine;
            }
            ObjectsText.text = objectStr;
            PopupBackgroundImage.texture = contentManager.Provider.GetAsset<TextureAsset>(new ResourceKey(0xA9600400, 0x499DB772, 0x856DDBAC)).Texture;
            BackgroundImage.texture = contentManager.Provider.GetAsset<TextureAsset>(new ResourceKey(0xCCC9AF70, 0x499DB772, 0x856DDBAC)).Texture;

            // Search for lot N001_Lot16.package (150 Main Street, in Pleasantview)
            string lotPath = "%UserDataDir%/Neighborhoods/N001/Lots/N001_Lot16.package";
            contentManager.Provider.AddPackage(lotPath);
            var lots = contentManager.Provider.GetAssetsOfType<LotAsset>(TypeIDs.LOT);
            var lot = lots.FirstOrDefault(x => x.FileName.Contains("N001_Lot16"));
            if (lot != null)
            {
                // place new game object representing lot
                var lotObject = new GameObject($"Lot:{lot.LotName}");
                var objts = contentManager.Provider.GetAssetsOfType<Game.Reimpl.TSSG.Object>(TypeIDs.OBJT).Where(x => x.TypeName == "cObject").ToArray();
                var objects = objts.Where(x => x.Package.FilePath == lot.FileName && !string.IsNullOrWhiteSpace(x.ModelName)).ToArray();
                // place each OBJT object in lot
                foreach (var objt in objects)
                {
                    Debug.Log($"Loading object {objt.ModelName} from \"{objt.Package.FilePath}\"");
                    var model = RCOLFile.GetModel(objt.ModelName);
                    if (model != null)
                    {
                        var gameObject = model.Spawn();
                        gameObject.transform.parent = lotObject.transform;
                        gameObject.transform.localPosition = new Vector3(objt.Coords.XCoord, objt.Coords.Height, objt.Coords.YCoord);
                        gameObject.transform.rotation = new Quaternion(objt.Coords.Rotation.x, -objt.Coords.Rotation.z, objt.Coords.Rotation.y, objt.Coords.Rotation.w);
                    }
                }
            }
        }
    }
}