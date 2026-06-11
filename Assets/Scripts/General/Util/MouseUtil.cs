using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//设为静态方法类，可直接使用
public static class MouseUtil
{
    private static Camera camera= Camera.main;
    public static Vector3 GetMousePositionInWordSpace(float zValue=0f)
    {
        //设置一个平面用于跟踪位置
        Plane dragPlane = new(camera.transform.forward,new Vector3(0,0,zValue));
        //从屏幕点创建一条射线，输入是鼠标的点击
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        //如果击中平面上的一点，则返回该点.  射线与平面相交，相交成功则把射线起点到交点的距离，存入distance
        if(dragPlane.Raycast(ray,out float distance))
        {
            //从射线起点走distance距离，得到交点的世界坐标->实现得到鼠标点击处的世界坐标
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
    
}
