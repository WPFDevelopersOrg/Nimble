using System.Windows;
using System.Windows.Controls.Primitives;

namespace SoftWareHelper.Helpers
{
    public class PopopHelper

    {

        public static DependencyObject GetPopupPlacementTarget(DependencyObject obj)

        {

            return (DependencyObject)obj.GetValue(PopupPlacementTargetProperty);

        }



        public static void SetPopupPlacementTarget(DependencyObject obj, DependencyObject value)

        {

            obj.SetValue(PopupPlacementTargetProperty, value);

        }



        public static readonly DependencyProperty PopupPlacementTargetProperty =

            DependencyProperty.RegisterAttached("PopupPlacementTarget", typeof(DependencyObject), typeof(PopopHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));



        private static void OnPopupPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)

        {

            if (e.NewValue != null)

            {

                DependencyObject popupPopupPlacementTarget = e.NewValue as DependencyObject;

                Popup pop = d as Popup;



                Window w = Window.GetWindow(popupPopupPlacementTarget);

                if (null != w)

                {

                    //让Popup随着窗体的移动而移动

                    w.LocationChanged += delegate

                    {

                        var mi = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        mi.Invoke(pop, null);

                    };

                    //让Popup随着窗体的Size改变而移动位置

                    w.SizeChanged += delegate

                    {

                        var mi = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        mi.Invoke(pop, null);

                    };

                }

            }

        }

    }
}
