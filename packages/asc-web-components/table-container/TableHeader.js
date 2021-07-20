import React from "react";
import PropTypes from "prop-types";
import throttle from "lodash.throttle";
import {
  StyledTableHeader,
  StyledTableRow,
  StyledEmptyTableContainer,
} from "./StyledTableContainer";
import Checkbox from "../checkbox";
import TableSettings from "./TableSettings";
import TableHeaderCell from "./TableHeaderCell";

const TABLE_SIZE = "tableSize";

class TableHeader extends React.Component {
  constructor(props) {
    super(props);

    this.state = { columnIndex: null };

    this.headerRef = React.createRef();
    this.throttledResize = throttle(this.onResize, 0);
  }

  componentDidMount() {
    this.onResize();
    window.addEventListener("resize", this.throttledResize);
  }

  componentWillUnmount() {
    window.removeEventListener("resize", this.throttledResize);
  }

  getSubstring = (str) => str.substring(0, str.length - 2);

  onMouseMove = (e) => {
    const { columnIndex } = this.state;
    const { containerRef } = this.props;
    if (!columnIndex) return;
    const column = document.getElementById("column_" + columnIndex);
    const columnSize = column.getBoundingClientRect();
    const newWidth = e.clientX - columnSize.left;

    const tableContainer = containerRef.current.style.gridTemplateColumns;
    const widths = tableContainer.split(" ");

    //getSubstring(widths[+columnIndex])
    if (newWidth <= 150) {
      widths[+columnIndex] = widths[+columnIndex];
    } else {
      const offset = +this.getSubstring(widths[+columnIndex]) - newWidth;
      const column2Width = +this.getSubstring(widths[+columnIndex + 1]);

      //getSubstring(widths[+columnIndex])
      if (column2Width + offset >= 150) {
        widths[+columnIndex] = newWidth + "px";
        widths[+columnIndex + 1] = column2Width + offset + "px";
      }
    }

    containerRef.current.style.gridTemplateColumns = widths.join(" ");
    this.headerRef.current.style.gridTemplateColumns = widths.join(" ");
  };

  onMouseUp = () => {
    localStorage.setItem(
      TABLE_SIZE,
      this.props.containerRef.current.style.gridTemplateColumns
    );

    window.removeEventListener("mousemove", this.onMouseMove);
    window.removeEventListener("mouseup", this.onMouseUp);
  };

  onMouseDown = (event) => {
    this.setState({ columnIndex: event.target.dataset.column });

    window.addEventListener("mousemove", this.onMouseMove);
    window.addEventListener("mouseup", this.onMouseUp);
  };

  onResize = () => {
    const { containerRef } = this.props;

    const container = containerRef.current
      ? containerRef.current
      : document.getElementById("table-container");

    const storageSize = localStorage.getItem(TABLE_SIZE);
    const tableContainer = storageSize
      ? storageSize.split(" ")
      : container.style.gridTemplateColumns.split(" ");

    const containerWidth = +container.clientWidth;
    const newContainerWidth = containerWidth - 32 - 80 - 24;

    let str = "";

    if (tableContainer.length > 1) {
      const gridTemplateColumns = [];

      const oldWidth = tableContainer
        .map((column) => +this.getSubstring(column))
        .reduce((x, y) => x + y);

      for (let index in tableContainer) {
        const item = tableContainer[index];

        if (item !== "24px" && item !== "32px" && item !== "80px") {
          const percent = (+this.getSubstring(item) / oldWidth) * 100;
          const newItemWidth = (containerWidth * percent) / 100 + "px";

          gridTemplateColumns.push(newItemWidth);
        } else {
          gridTemplateColumns.push(item);
        }

        str = gridTemplateColumns.join(" ");
      }
    } else {
      const column = (newContainerWidth * 40) / 100 + "px";
      const otherColumns = (newContainerWidth * 20) / 100 + "px";

      str = `32px ${column} ${otherColumns} ${otherColumns} ${otherColumns} 80px 24px`;
    }
    container.style.gridTemplateColumns = str;
    this.headerRef.current.style.gridTemplateColumns = str;
    this.headerRef.current.style.width = containerWidth + "px";

    localStorage.setItem(TABLE_SIZE, str);
  };

  onChange = (checked) => {
    this.props.setSelected(checked ? "all" : "none");
  };

  render() {
    const { columns, ...rest } = this.props;

    return (
      <>
        <StyledTableHeader
          className="table-container_header"
          ref={this.headerRef}
          {...rest}
        >
          <StyledTableRow>
            <Checkbox onChange={this.onChange} isChecked={false} />

            {columns.map((column, index) => {
              return (
                column.enable && (
                  <TableHeaderCell
                    key={column.key}
                    index={index}
                    column={column}
                    onMouseDown={this.onMouseDown}
                  />
                )
              );
            })}

            <div className="table-container_header-cell">
              <TableSettings columns={columns} />
            </div>
          </StyledTableRow>
        </StyledTableHeader>
        <StyledEmptyTableContainer />
      </>
    );
  }
}

TableHeader.propTypes = {
  containerRef: PropTypes.shape({ current: PropTypes.any }),
  columns: PropTypes.array.isRequired,
  setSelected: PropTypes.func,
};

export default TableHeader;
