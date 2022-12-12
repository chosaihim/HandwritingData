using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class BarracudaManager : MonoBehaviour
{
    public NNModel modelAsset;
    private Model m_RuntimeModel;
    // Start is called before the first frame update
    void Start()
    {
        m_RuntimeModel = ModelLoader.Load(modelAsset);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void inputData (string fullLineParam) {
        Debug.Log(fullLineParam);

        IWorker worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);
        
        float[,] jaggedArray = new float[50 , 2] {{-0.5f,0.1800477F},
                                            {-0.46410879f,0.19375854f},
                                            {-0.41686952f,0.20688989f},
                                            {-0.37127543f,0.21670499f},
                                            {-0.33873411f,0.22091247f},
                                            {-0.30473149f,0.22516279f},
                                            {-0.26601671f,0.22953404f},
                                            {-0.22700621f,0.23380587f},
                                            {-0.19054366f,0.2380064f},
                                            {-0.14985589f,0.24528808f},
                                            {-0.12323309f,0.25016709f},
                                            {-0.10129457f,0.25339767f},
                                            {-0.07962478f,0.25299343f},
                                            {-0.06972659f,0.22400302f},
                                            {-0.08066652f,0.18410833f},
                                            {-0.09765948f,0.14174947f},
                                            {-0.11383961f,0.10045859f},
                                            {-0.13029728f,0.05670874f},
                                            {-0.14711414f,-0.00315629f},
                                            {-0.16232419f,-0.07323586f},
                                            {-0.17544026f,-0.14989859f},
                                            {-0.18799927f,-0.22525641f},
                                            {-0.19636838f,-0.28768834f},
                                            {-0.20308187f,-0.33779363f},
                                            {-0.14652737f,-0.26547939f},
                                            {0.23321427f,0.38855859f},
                                            {0.29102586f,0.47197267f},
                                            {0.28760088f,0.43650613f},
                                            {0.28518533f,0.39122416f},
                                            {0.28274137f,0.33813263f},
                                            {0.2842816f,0.27601188f},
                                            {0.28743048f,0.21032137f},
                                            {0.29189594f,0.14262354f},
                                            {0.29809548f,0.07430139f},
                                            {0.30411386f,0.00128391f},
                                            {0.308755f,-0.07454427f},
                                            {0.31207159f,-0.15026638f},
                                            {0.31447984f,-0.22034514f},
                                            {0.31690911f,-0.28158334f},
                                            {0.31933552f,-0.3363051f},
                                            {0.32231468f,-0.3836742f},
                                            {0.32679492f,-0.42175128f},
                                            {0.33229974f,-0.45278714f},
                                            {0.33890468f,-0.47857277f},
                                            {0.33369842f,-0.38297958f},
                                            {0.29907623f,-0.00746584f},
                                            {0.35406209f,0.00882982f},
                                            {0.40472346f,0.01237221f},
                                            {0.45202112f,0.01516377f},
                                            {0.5f,0.01952262f}};

        var tensor = new Tensor(1, 1, 2, 50);  
        var tensor2 = new Tensor(1, 1, 2, 50);  

        //정 방향
        for (int i = 0; i < jaggedArray.GetLength(0); i++)
        {
            for (int j = 0; j < jaggedArray.GetLength(1); j++) {
                float k = jaggedArray[i,j];
                tensor[0, 0, j, i] = k;
            }
        }

        //역 방향
        int num = 0;
        for (int i = jaggedArray.GetLength(0) -1 ; i >= 0; i--)
        {
            for (int j = 0; j < jaggedArray.GetLength(1); j++) {
                float k = jaggedArray[i,j];
                tensor2[0, 0, j, num] = k;
            }
            num++;
        }
     
        Debug.Log(tensor);
        Debug.Log(tensor2);

        worker.Execute(tensor);
        var output = worker.PeekOutput();
        Debug.Log(output);
        var res = output.ArgMax()[0];
        Debug.Log(output[0]);
        worker.Execute(tensor2);
        var output2 = worker.PeekOutput();
        Debug.Log(output2);
        var res2 = output2.ArgMax()[0];
        Debug.Log(output2[0]);
        worker.Dispose();

        Debug.Log(res);
    }

  

}
