using System.Collections;
using System.Collections.Generic;
using _Projects.GamePlay;
using UnityEngine;

public class BoxObject : DisposableObject
{
    public GameObject boxPrefab;
    public GameObject openedBoxPrefab;
    public GameObject bombPrefab; // 炸弹预制体
    public float bombForce = 5f; // 弹出力度

    public bool isOpened = false;
    private bool hasSpawnedBomb = false; // 是否已经生成过炸弹
    
    public override void ChangeState()
    {
        if (!isOpened)
        {
            boxPrefab.SetActive(false);
            openedBoxPrefab.SetActive(true);
            isOpened = true;
            // 第一次打开时生成炸弹
            if (!hasSpawnedBomb && bombPrefab != null)
            {
                SpawnBomb();
                hasSpawnedBomb = true;
            }
        }
        base.ChangeState();
        
    }

    public override void Recycle()
    {
        if (isOpened)
        {
            boxPrefab.SetActive(true);
            openedBoxPrefab.SetActive(false);
            isOpened = false;
        }
        base.Recycle();
    }
    
    /// <summary>
    /// 生成炸弹并施加弹出力
    /// </summary>
    private void SpawnBomb()
    {
        // 在盒子位置上方一点生成炸弹
        Vector3 spawnPosition = openedBoxPrefab.transform.position + Vector3.up * 0.5f;
        GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        
        // 如果炸弹有 Rigidbody，施加向上的力
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 添加向上的力，带一点随机偏移
            Vector3 force = Vector3.up * bombForce;
            force.x += Random.Range(-1f, 1f);
            force.z += Random.Range(-1f, 1f);
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}

