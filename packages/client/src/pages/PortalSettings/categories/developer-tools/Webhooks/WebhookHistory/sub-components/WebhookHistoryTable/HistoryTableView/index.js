import React, { useRef, useEffect } from "react";

import styled from "styled-components";

import { isMobile } from "react-device-detect";

import TableContainer from "@docspace/components/table-container/TableContainer";
import TableBody from "@docspace/components/table-container/TableBody";
import HistoryTableHeader from "./HistoryTableHeader";
import { HistoryTableRow } from "./HistoryTableRow";

import { inject, observer } from "mobx-react";

const TableWrapper = styled(TableContainer)`
  margin-top: 5px;

  .table-container_row-checkbox {
    padding-left: 20px;
    width: 16px;
  }
`;

const HistoryTableView = (props) => {
  const { sectionWidth, historyWebhooks, viewAs, setViewAs } = props;

  const tableRef = useRef(null);

  useEffect(() => {
    if (!sectionWidth) return;
    if (sectionWidth < 1025 || isMobile) {
      viewAs !== "row" && setViewAs("row");
    } else {
      viewAs !== "table" && setViewAs("table");
    }
  }, [sectionWidth]);

  return (
    <TableWrapper
      forwardedRef={tableRef}
      style={{
        gridTemplateColumns: "300px 100px 400px 24px",
      }}>
      <HistoryTableHeader sectionWidth={sectionWidth} tableRef={tableRef} />
      <TableBody itemHeight={49}>
        {historyWebhooks.map((item) => (
          <HistoryTableRow key={item.id} eventData={item} />
        ))}
      </TableBody>
    </TableWrapper>
  );
};

export default inject(({ setup }) => {
  const { viewAs, setViewAs } = setup;
  return {
    viewAs,
    setViewAs,
  };
})(observer(HistoryTableView));
