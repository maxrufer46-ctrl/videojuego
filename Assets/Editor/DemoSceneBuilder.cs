#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class DemoSceneBuilder
{
    [MenuItem("Street Heat/Build Demo Scene")]
    public static void BuildDemoScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "RoadGround";
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        for (int i = -4; i <= 4; i++)
        {
            CreateBarrier(new Vector3(-11f, 0.6f, i * 10f), new Vector3(1f, 1.2f, 9f));
            CreateBarrier(new Vector3(11f, 0.6f, i * 10f), new Vector3(1f, 1.2f, 9f));
        }

        GameObject player = CreateCar("PlayerCar", new Vector3(0f, 0.8f, -25f), false);
        var car = player.AddComponent<ArcadeCarController>();
        var heat = player.AddComponent<HeatSystem>();

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 72f;
        var chase = camGo.AddComponent<ChaseCamera>();
        chase.target = player.transform;
        camGo.transform.position = player.transform.position + new Vector3(0f, 4f, -8f);

        GameObject police = CreateCar("PoliceCar", new Vector3(0f, 0.8f, -42f), true);
        var ai = police.AddComponent<PoliceChaseAI>();
        ai.target = player.transform;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        Canvas canvas = CreateCanvas();
        Text speed = CreateText(canvas.transform, "SpeedText", "0 km/h", new Vector2(0.82f, 0.88f), 34);
        Text heatText = CreateText(canvas.transform, "HeatText", "HEAT 1", new Vector2(0.14f, 0.90f), 30);

        Image nitroBack = CreateImage(canvas.transform, "NitroBack", new Vector2(0.82f, 0.82f), new Vector2(170f, 18f), new Color(0f,0f,0f,0.6f));
        Image nitroFill = CreateImage(nitroBack.transform, "NitroFill", new Vector2(0.5f, 0.5f), new Vector2(164f, 12f), new Color(0.2f,0.75f,1f,1f));
        nitroFill.type = Image.Type.Filled;
        nitroFill.fillMethod = Image.FillMethod.Horizontal;
        nitroFill.fillAmount = 1f;

        CreateHoldButton(canvas.transform, "LEFT", new Vector2(0.09f, 0.12f), HoldButton.ControlType.SteerLeft);
        CreateHoldButton(canvas.transform, "RIGHT", new Vector2(0.22f, 0.12f), HoldButton.ControlType.SteerRight);
        CreateHoldButton(canvas.transform, "BRAKE", new Vector2(0.73f, 0.12f), HoldButton.ControlType.Brake);
        CreateHoldButton(canvas.transform, "GAS", new Vector2(0.88f, 0.12f), HoldButton.ControlType.Throttle);
        CreateHoldButton(canvas.transform, "NITRO", new Vector2(0.87f, 0.28f), HoldButton.ControlType.Nitro);
        CreateHoldButton(canvas.transform, "DRIFT", new Vector2(0.70f, 0.28f), HoldButton.ControlType.Handbrake);

        var hud = canvas.gameObject.AddComponent<HUDController>();
        hud.car = car;
        hud.heat = heat;
        hud.speedText = speed;
        hud.heatText = heatText;
        hud.nitroFill = nitroFill;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Demo.unity");
        Selection.activeGameObject = player;
        Debug.Log("Street Heat demo created at Assets/Scenes/Demo.unity");
    }

    private static GameObject CreateCar(string name, Vector3 position, bool police)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = police ? 1350f : 1250f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.9f, 0.75f, 4.2f);
        collider.center = new Vector3(0f, 0.35f, 0f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        body.transform.localScale = new Vector3(1.9f, 0.65f, 4.1f);
        Object.DestroyImmediate(body.GetComponent<Collider>());

        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.transform.SetParent(root.transform);
        cabin.transform.localPosition = new Vector3(0f, 0.95f, -0.15f);
        cabin.transform.localScale = new Vector3(1.55f, 0.55f, 1.8f);
        Object.DestroyImmediate(cabin.GetComponent<Collider>());

        if (police)
        {
            GameObject lightbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightbar.transform.SetParent(root.transform);
            lightbar.transform.localPosition = new Vector3(0f, 1.35f, -0.05f);
            lightbar.transform.localScale = new Vector3(1.3f, 0.12f, 0.25f);
            Object.DestroyImmediate(lightbar.GetComponent<Collider>());
        }

        return root;
    }

    private static void CreateBarrier(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = pos;
        go.transform.localScale = scale;
    }

    private static Canvas CreateCanvas()
    {
        var go = new GameObject("MobileHUD");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 anchor, int size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(240f, 70f);
        var text = go.AddComponent<Text>();
        text.text = value;
        try { text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        catch { text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return text;
    }

    private static Image CreateImage(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static void CreateHoldButton(Transform parent, string label, Vector2 anchor, HoldButton.ControlType type)
    {
        var image = CreateImage(parent, label + "Button", anchor, new Vector2(155f, 155f), new Color(0f, 0f, 0f, 0.5f));
        var buttonLogic = image.gameObject.AddComponent<HoldButton>();
        buttonLogic.control = type;
        Text txt = CreateText(image.transform, "Label", label, new Vector2(0.5f, 0.5f), 26);
        txt.GetComponent<RectTransform>().sizeDelta = image.GetComponent<RectTransform>().sizeDelta;
    }
}
#endif
