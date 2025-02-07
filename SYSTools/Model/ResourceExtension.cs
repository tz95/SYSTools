using System;
using System.Globalization;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Data;

namespace SYSTools.Model
{
    public class ResourceExtension : MarkupExtension
    {
        private static event EventHandler LanguageChanged;
        private object _targetObject;
        private object _targetProperty;
        
        public string Key { get; set; }

        public static void NotifyLanguageChanged()
        {
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
                return string.Empty;

            // 保存目标对象和属性
            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget != null)
            {
                _targetObject = provideValueTarget.TargetObject;
                _targetProperty = provideValueTarget.TargetProperty;
            }

            // 订阅语言变更事件
            LanguageChanged += OnLanguageChanged;

            return GetResourceString();
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (_targetObject != null && _targetProperty != null)
            {
                // 在UI线程上更新值
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_targetObject is DependencyObject targetObject && _targetProperty is DependencyProperty targetProperty)
                    {
                        targetObject.SetValue(targetProperty, GetResourceString());
                    }
                });
            }
        }

        private string GetResourceString()
        {
            return Properties.Lang.ResourceManager.GetString(Key, CultureInfo.CurrentUICulture) ?? string.Empty;
        }
    }
}
