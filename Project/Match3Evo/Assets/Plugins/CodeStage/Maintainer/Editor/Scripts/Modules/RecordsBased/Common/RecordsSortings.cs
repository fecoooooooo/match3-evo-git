﻿#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer
{
	using System;

	using Cleaner;
	using Issues;

	internal static class RecordsSortings
	{
		internal static readonly Func<CleanerRecord, string>		cleanerRecordByPath = record => record is AssetRecord ? ((AssetRecord)record).path : null;
		internal static readonly Func<CleanerRecord, long>			cleanerRecordBySize = record => record is AssetRecord ? ((AssetRecord)record).size : 0;
		internal static readonly Func<CleanerRecord, RecordType>	cleanerRecordByType = record => record.type;
		internal static readonly Func<CleanerRecord, string>		cleanerRecordByAssetType = record => record is AssetRecord ? ((AssetRecord)record).type == RecordType.UnreferencedAsset ? ((AssetRecord)record).assetType.FullName : null : null;

		internal static readonly Func<IssueRecord, string>			issueRecordByPath = record => record is GameObjectIssueRecord ? ((GameObjectIssueRecord)record).Path : null;
		internal static readonly Func<IssueRecord, IssueKind>		issueRecordByType = record => record.Kind;
		internal static readonly Func<IssueRecord, RecordSeverity>	issueRecordBySeverity = record => record.Severity;
	}
}