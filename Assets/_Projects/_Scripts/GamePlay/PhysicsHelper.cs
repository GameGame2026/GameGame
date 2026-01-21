using UnityEngine;

/// <summary>
/// 物理检测工具类 - 提供底层物理检测方法
/// 职责：封装Unity物���系统的基础检测功能
/// </summary>
public static class PhysicsHelper
{
    /// <summary>
    /// 球形投射检测地面
    /// </summary>
    public static bool CheckGroundSphere(Vector3 position, float radius, float distance, LayerMask groundLayer, out RaycastHit hitInfo)
    {
        return Physics.SphereCast(
            position,
            radius,
            Vector3.down,
            out hitInfo,
            distance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    /// <summary>
    /// 射线检测地面
    /// </summary>
    public static bool CheckGroundRay(Vector3 position, float distance, LayerMask groundLayer, out RaycastHit hitInfo)
    {
        return Physics.Raycast(
            position,
            Vector3.down,
            out hitInfo,
            distance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    /// <summary>
    /// 胶囊体投射检测地面
    /// </summary>
    public static bool CheckGroundCapsule(Vector3 point1, Vector3 point2, float radius, float distance, LayerMask groundLayer, out RaycastHit hitInfo)
    {
        return Physics.CapsuleCast(
            point1,
            point2,
            radius,
            Vector3.down,
            out hitInfo,
            distance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    /// <summary>
    /// 检测指定方向的障碍物
    /// </summary>
    public static bool CheckObstacle(Vector3 position, Vector3 direction, float distance, LayerMask obstacleLayer, out RaycastHit hitInfo)
    {
        return Physics.Raycast(
            position,
            direction,
            out hitInfo,
            distance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    /// <summary>
    /// 检测台阶
    /// </summary>
    public static bool CheckStep(Vector3 position, Vector3 forward, float radius, float stepHeight, float stepDistance, LayerMask groundLayer, out float stepUpHeight)
    {
        stepUpHeight = 0;

        // 检测前方是否有障碍
        Vector3 checkStart = position + Vector3.up * 0.1f;
        if (!Physics.Raycast(checkStart, forward, radius + stepDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        // 检测台阶顶部
        Vector3 stepCheckPos = checkStart + forward * (radius + stepDistance) + Vector3.up * stepHeight;
        if (!Physics.Raycast(stepCheckPos, Vector3.down, out RaycastHit stepHit, stepHeight, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        stepUpHeight = stepHit.point.y - position.y;
        return stepUpHeight > 0.01f && stepUpHeight <= stepHeight;
    }

    /// <summary>
    /// 检测斜坡角度
    /// </summary>
    public static bool CheckSlope(Vector3 position, float checkDistance, LayerMask groundLayer, out float slopeAngle)
    {
        slopeAngle = 0;

        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, checkDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检测范围内的碰撞体（球形）
    /// </summary>
    public static Collider[] OverlapSphere(Vector3 position, float radius, LayerMask layer)
    {
        return Physics.OverlapSphere(position, radius, layer, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// 检测范围内的碰撞体（胶囊体）
    /// </summary>
    public static Collider[] OverlapCapsule(Vector3 point1, Vector3 point2, float radius, LayerMask layer)
    {
        return Physics.OverlapCapsule(point1, point2, radius, layer, QueryTriggerInteraction.Ignore);
    }
}

