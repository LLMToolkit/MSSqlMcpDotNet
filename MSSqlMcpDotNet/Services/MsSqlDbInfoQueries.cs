namespace MSSqlMcpDotNet.Services;


public partial class MsSqlDbInfo
{

    string qryDatabases = @"
SELECT 
    d.name AS DatabaseName,
    d.state_desc AS State,
    d.recovery_model_desc AS RecoveryModel,
    d.compatibility_level AS CompatibilityLevel,
    d.collation_name AS Collation,
    d.create_date AS CreateDate,
    d.user_access_desc AS UserAccess,
    d.is_read_only AS IsReadOnly,
    d.is_auto_close_on AS IsAutoCloseOn,
	d.is_auto_shrink_on as IsAutoShrink,
    d.is_encrypted AS IsEncrypted,	
	d.snapshot_isolation_state_desc as SnapshotIsolationState,
	d.is_read_committed_snapshot_on as IsReadCommittedSnapshot,
    SUM(mf.size) / 128 AS SizeMB
FROM sys.databases d
LEFT JOIN sys.master_files mf ON d.database_id = mf.database_id
WHERE d.database_id > 4
GROUP BY 
    d.name,
    d.state_desc,
    d.recovery_model_desc,
    d.compatibility_level,
    d.collation_name,
    d.create_date,
    d.user_access_desc,
    d.is_read_only,
    d.is_auto_close_on,
    d.is_encrypted,
	d.is_auto_shrink_on,
	d.snapshot_isolation_state_desc,
	d.is_read_committed_snapshot_on
ORDER BY d.name;

";


    string qryTables = @"
WITH RowCounts AS
(
    SELECT 
        p.object_id,
        SUM(p.rows) AS rows
    FROM sys.partitions p
    WHERE p.index_id IN (0,1) -- heap o clustered index
    GROUP BY p.object_id
),
SpaceUsed AS
(
    SELECT 
        ps.object_id,
        SUM(ps.reserved_page_count) * 8.0 / 1024 AS reserved_mb,
        SUM(ps.used_page_count) * 8.0 / 1024 AS used_mb
    FROM sys.dm_db_partition_stats ps
    GROUP BY ps.object_id
)
SELECT 
    s.name AS [schema],
	t.*,
    --t.name AS TableName,
    --t.create_date AS CreateDate,
    --t.modify_date AS ModifyDate,
    --t.is_ms_shipped AS IsSystemTable,
    --t.is_memory_optimized AS IsMemoryOptimized,
    --t.is_external AS IsExternal,
    rc.rows,
    su.reserved_mb,
    su.used_mb
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN RowCounts rc ON t.object_id = rc.object_id
LEFT JOIN SpaceUsed su ON t.object_id = su.object_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

";


    string qryViews = @"
SELECT
    s.name AS[schema],
v.*
FROM sys.views v
    INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
    WHERE v.is_ms_shipped = 0
ORDER BY s.name, v.name;

";


    string qryTableColumns = @"
WITH PKColumns AS
(
    SELECT ic.object_id, ic.column_id, kc.name AS PKConstraintName
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.key_constraints kc ON i.object_id = kc.parent_object_id AND i.name = kc.name AND kc.type = 'PK'
    WHERE i.is_primary_key = 1
)

SELECT 
    c.column_id AS ColumnOrder,
    c.name AS ColumnName,
    t.name AS DataType,
    CASE 
        WHEN t.name IN ('char', 'varchar', 'nchar', 'nvarchar') 
             THEN CASE WHEN c.max_length = -1 THEN 'MAX' 
                       ELSE CAST(c.max_length / CASE WHEN t.name IN ('nchar','nvarchar') THEN 2 ELSE 1 END AS VARCHAR) 
                  END
        WHEN t.name IN ('decimal','numeric') 
             THEN CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR)
        WHEN t.name IN ('float','real') 
             THEN CAST(c.precision AS VARCHAR)
        ELSE ''
    END AS Size,
    CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS IsNullable,
    CASE WHEN pk.PKConstraintName IS NOT NULL THEN 'YES' ELSE 'NO' END AS IsPrimaryKey,
    pk.PKConstraintName,
    CASE WHEN c.is_identity = 1 THEN 'YES' ELSE 'NO' END AS IsIdentity,
    CASE WHEN c.is_computed = 1 THEN 'YES' ELSE 'NO' END AS IsComputed,
    cc.definition AS ComputedDefinition,
    dc.definition AS DefaultValue,
    ep.value AS ColumnDescription
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
INNER JOIN sys.tables tbl ON c.object_id = tbl.object_id
INNER JOIN sys.schemas s ON tbl.schema_id = s.schema_id
LEFT JOIN PKColumns pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
LEFT JOIN sys.extended_properties ep 
    ON c.object_id = ep.major_id 
    AND c.column_id = ep.minor_id 
    AND ep.name = 'MS_Description'
WHERE tbl.name = @tableName
  AND s.name = @schema
ORDER BY c.column_id;

            ";
    
    

    string qryTableReferentialIntegrity = @"
SELECT 
    SCHEMA_NAME(tp.schema_id) AS TableSchema,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    c1.name AS ColumnName,
    fk.name AS ForeignKeyName,
    SCHEMA_NAME(tr.schema_id) AS ReferencedTableSchema,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    c2.name AS ReferencedColumnName,
    delete_referential_action_desc AS delete_action,
    update_referential_action_desc AS update_action
FROM 
    sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id 
                          AND fkc.parent_column_id = c1.column_id
INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id 
                          AND fkc.referenced_column_id = c2.column_id
INNER JOIN sys.objects tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.objects tr ON fk.referenced_object_id = tr.object_id
WHERE 
    (   OBJECT_NAME(fk.parent_object_id) = @TableName 
    AND SCHEMA_NAME(tp.schema_id) = @schema
    )
    OR 
    (   OBJECT_NAME(fk.referenced_object_id) = @TableName 
    AND SCHEMA_NAME(tr.schema_id) = @schema
    )
ORDER BY 
    TableSchema, TableName, ForeignKeyName;

";


    private string qryViewColumns = @"
SELECT 
    s.name AS SchemaName,
    v.name AS ViewName,
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
	CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS IsNullable
FROM sys.views v
INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
INNER JOIN sys.columns c ON v.object_id = c.object_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE v.name = @ViewName
  AND s.name = @SchemaName
ORDER BY SchemaName, ViewName, c.column_id;
    ";





}