import type { ReactNode } from "react"
import {
  type ColumnDef,
  type Table as TanStackTable,
  type Row,
  flexRender,
} from "@tanstack/react-table"
import { IconArrowsSort } from "@tabler/icons-react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"

interface DataTableProps<TData, TValue> {
  table: TanStackTable<TData>
  columns: ColumnDef<TData, TValue>[]
  emptyMessage?: string
  onRowClick?: (row: TData, tanstackRow: Row<TData>) => void
  rowClassName?: (row: TData) => string
  renderSubRow?: (row: TData) => ReactNode
  headerStyle?: (headerId: string, size: number) => React.CSSProperties | undefined
}

export function DataTable<TData, TValue>({
  table,
  columns,
  emptyMessage = "No results.",
  onRowClick,
  rowClassName,
  renderSubRow,
  headerStyle,
}: DataTableProps<TData, TValue>) {
  return (
    <div className="border">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => {
                const canSort = header.column.getCanSort()
                return (
                  <TableHead
                    key={header.id}
                    className={canSort ? "cursor-pointer select-none" : ""}
                    onClick={canSort ? header.column.getToggleSortingHandler() : undefined}
                    style={headerStyle?.(header.id, header.getSize())}
                  >
                    <div className="flex items-center gap-1">
                      {header.isPlaceholder
                        ? null
                        : flexRender(
                            header.column.columnDef.header,
                            header.getContext(),
                          )}
                      {canSort && (
                        <IconArrowsSort className="h-3 w-3 text-muted-foreground" />
                      )}
                    </div>
                  </TableHead>
                )
              })}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {table.getRowModel().rows.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={columns.length}
                className="h-24 text-center text-muted-foreground"
              >
                {emptyMessage}
              </TableCell>
            </TableRow>
          ) : (
            table.getRowModel().rows.map((row) => (
              <DataTableRow
                key={row.id}
                row={row}
                columns={columns}
                onRowClick={onRowClick}
                rowClassName={rowClassName}
                renderSubRow={renderSubRow}
              />
            ))
          )}
        </TableBody>
      </Table>
    </div>
  )
}

function DataTableRow<TData, TValue>({
  row,
  columns,
  onRowClick,
  rowClassName,
  renderSubRow,
}: {
  row: Row<TData>
  columns: ColumnDef<TData, TValue>[]
  onRowClick?: (row: TData, tanstackRow: Row<TData>) => void
  rowClassName?: (row: TData) => string
  renderSubRow?: (row: TData) => ReactNode
}) {
  return (
    <>
      <TableRow
        className={[
          onRowClick ? "cursor-pointer" : "",
          rowClassName?.(row.original) ?? "",
        ]
          .filter(Boolean)
          .join(" ")}
        data-state={row.getIsExpanded() ? "selected" : undefined}
        onClick={onRowClick ? () => onRowClick(row.original, row) : undefined}
      >
        {row.getVisibleCells().map((cell) => (
          <TableCell key={cell.id}>
            {flexRender(cell.column.columnDef.cell, cell.getContext())}
          </TableCell>
        ))}
      </TableRow>
      {renderSubRow && row.getIsExpanded() && (
        <TableRow>
          <TableCell colSpan={columns.length} className="p-0">
            {renderSubRow(row.original)}
          </TableCell>
        </TableRow>
      )}
    </>
  )
}
