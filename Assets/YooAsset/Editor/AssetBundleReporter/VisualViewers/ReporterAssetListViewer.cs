#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;

namespace YooAsset.Editor
{
    internal class ReporterAssetListViewer
    {
        private class AssetTableData : DefaultTableData
        {
            public ReportAssetInfo AssetInfo;
        }
        private class DependTableData : DefaultTableData
        {
            public ReportBundleInfo BundleInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableView _assetTableView;
        private TableView _dependTableView;

        private BuildReport _buildReport;
        private string _reportFilePath;
        private List<ITableData> _sourceDatas;


        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUXML<ReporterAssetListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源列表
            _assetTableView = _root.Q<TableView>("TopTableView");
            _assetTableView.SelectionChangedEvent = OnAssetTableViewSelectionChanged;
            _assetTableView.ClickTableDataEvent = OnClickAssetTableView;
            CreateAssetTableViewColumns();

            // 依赖列表
            _dependTableView = _root.Q<TableView>("BottomTableView");
            _dependTableView.ClickTableDataEvent = OnClickBundleTableView;
            CreateDependTableViewColumns();

#if UNITY_2020_3_OR_NEWER
            SplitView.Adjuster(_root);
#endif
        }
        private void CreateAssetTableViewColumns()
        {
            // AssetPath
            {
                var columnStyle = new ColumnStyle();
                columnStyle.Width = 300;
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("AssetPath", "AssetPath", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.marginLeft = 3f;
                    label.style.flexGrow = columnStyle.Stretchable ? 1f : 0f;
                    label.style.width = columnStyle.Width;
                    label.style.maxWidth = columnStyle.MaxWidth;
                    label.style.minWidth = columnStyle.MinWidth;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _assetTableView.AddColumn(column);
            }

            //MainBundle
            {
                var columnStyle = new ColumnStyle();
                columnStyle.Width = 150;
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("MainBundle", "MainBundle", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.marginLeft = 3f;
                    label.style.flexGrow = columnStyle.Stretchable ? 1f : 0f;
                    label.style.width = columnStyle.Width;
                    label.style.maxWidth = columnStyle.MaxWidth;
                    label.style.minWidth = columnStyle.MinWidth;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _assetTableView.AddColumn(column);
            }
        }
        private void CreateDependTableViewColumns()
        {
            // DependBundles
            {
                var columnStyle = new ColumnStyle();
                columnStyle.Width = 280;
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("DependBundles", "DependBundles", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.marginLeft = 3f;
                    label.style.flexGrow = columnStyle.Stretchable ? 1f : 0f;
                    label.style.width = columnStyle.Width;
                    label.style.maxWidth = columnStyle.MaxWidth;
                    label.style.minWidth = columnStyle.MinWidth;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _dependTableView.AddColumn(column);
            }

            // FileSize
            {
                var columnStyle = new ColumnStyle();
                columnStyle.Width = 100;
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("FileSize", "FileSize", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.marginLeft = 3f;
                    label.style.flexGrow = columnStyle.Stretchable ? 1f : 0f;
                    label.style.width = columnStyle.Width;
                    label.style.maxWidth = columnStyle.MaxWidth;
                    label.style.minWidth = columnStyle.MinWidth;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    long fileSize = (long)cell.CellValue;
                    infoLabel.text = EditorUtility.FormatBytes(fileSize);
                };
                _dependTableView.AddColumn(column);
            }

            // FileHash
            {
                var columnStyle = new ColumnStyle();
                columnStyle.Width = 250;
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("FileHash", "FileHash", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.marginLeft = 3f;
                    label.style.flexGrow = columnStyle.Stretchable ? 1f : 0f;
                    label.style.width = columnStyle.Width;
                    label.style.maxWidth = columnStyle.MaxWidth;
                    label.style.minWidth = columnStyle.MinWidth;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _dependTableView.AddColumn(column);
            }
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport, string reprotFilePath)
        {
            _buildReport = buildReport;
            _reportFilePath = reprotFilePath;

            // 清空旧数据
            _assetTableView.ClearAll(false, true);
            _dependTableView.ClearAll(false, true);

            // 填充数据源
            _sourceDatas = new List<ITableData>(_buildReport.AssetInfos.Count);
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                var rowData = new AssetTableData();
                rowData.AssetInfo = assetInfo;
                rowData.AddAssetPathCell("AssetPath", assetInfo.AssetPath);
                rowData.AddStringValueCell("MainBundle", assetInfo.MainBundleName);
                _sourceDatas.Add(rowData);
            }
            _assetTableView.itemsSource = _sourceDatas;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 重建视图
        /// </summary>
        public void RebuildView(string searchKeyWord)
        {
            // 搜索匹配
            DefaultSearchSystem.Search(_sourceDatas, searchKeyWord);

            // 重建视图
            _assetTableView.RebuildView();
        }

        /// <summary>
        /// 挂接到父类页面上
        /// </summary>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 从父类页面脱离开
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }

        private void OnAssetTableViewSelectionChanged(ITableData data)
        {
            var assetTableData = data as AssetTableData;
            ReportAssetInfo assetInfo = assetTableData.AssetInfo;

            // 填充依赖数据
            var mainBundle = _buildReport.GetBundleInfo(assetInfo.MainBundleName);
            var sourceDatas = new List<ITableData>(mainBundle.DependBundles.Count);
            foreach (string dependBundleName in mainBundle.DependBundles)
            {
                var dependBundle = _buildReport.GetBundleInfo(dependBundleName);
                var rowData = new DependTableData();
                rowData.BundleInfo = dependBundle;
                rowData.AddStringValueCell("DependBundles", dependBundle.BundleName);
                rowData.AddLongValueCell("FileSize", dependBundle.FileSize);
                rowData.AddStringValueCell("FileHash", dependBundle.FileHash);
                sourceDatas.Add(rowData);
            }
            _dependTableView.itemsSource = sourceDatas;
            _dependTableView.RebuildView();

            // 刷新标题
            string headerTitle = $"DependBundles ({mainBundle.DependBundles.Count})";
            _dependTableView.SetHeaderTitle("DependBundles", headerTitle);
        }
        private void OnClickAssetTableView(PointerDownEvent evt, ITableData data)
        {
            // 鼠标双击后检视
            if (evt.clickCount != 2)
                return;

            foreach (var cell in data.Cells)
            {
                if (cell is AssetPathCell assetPathCell)
                {
                    if (assetPathCell.PingAssetObject())
                        break;
                }
            }
        }
        private void OnClickBundleTableView(PointerDownEvent evt, ITableData data)
        {
            // 鼠标双击后检视
            if (evt.clickCount != 2)
                return;

            var dependTableData = data as DependTableData;
            if (dependTableData.BundleInfo.Encrypted)
                return;

            if (_buildReport.Summary.BuildBundleType == (int)EBuildBundleType.AssetBundle)
            {
                string rootDirectory = Path.GetDirectoryName(_reportFilePath);
                string filePath = $"{rootDirectory}/{dependTableData.BundleInfo.FileName}";
                if (File.Exists(filePath))
                    Selection.activeObject = AssetBundleRecorder.GetAssetBundle(filePath);
                else
                    Selection.activeObject = null;
            }
        }
    }
}
#endif