import React from "react";
import PropTypes from "prop-types";
import { SearchInput } from "asc-web-components";
import isEqual from "lodash/isEqual";
import FilterBlock from "./sub-components/FilterBlock";
import SortComboBox from "./sub-components/SortComboBox";
import ViewSelector from "./sub-components/ViewSelector";
import map from "lodash/map";
import clone from "lodash/clone";
import StyledFilterInput from "./StyledFilterInput";

const cloneObjectsArray = function (props) {
  return map(props, clone);
};
const convertToInternalData = function (fullDataArray, inputDataArray) {
  const filterItems = [];
  for (let i = 0; i < inputDataArray.length; i++) {
    let filterValue = fullDataArray.find(
      (x) =>
        x.key ===
          inputDataArray[i].key.replace(inputDataArray[i].group + "_", "") &&
        x.group === inputDataArray[i].group &&
        !x.inSubgroup
    );
    if (filterValue) {
      inputDataArray[i].key =
        inputDataArray[i].group + "_" + inputDataArray[i].key;
      inputDataArray[i].label = filterValue.label;
      inputDataArray[i].groupLabel = !fullDataArray.inSubgroup
        ? fullDataArray.find((x) => x.group === inputDataArray[i].group).label
        : inputDataArray[i].groupLabel;
      filterItems.push(inputDataArray[i]);
    } else {
      filterValue = fullDataArray.find(
        (x) =>
          x.key ===
            inputDataArray[i].key.replace(inputDataArray[i].group + "_", "") &&
          x.group === inputDataArray[i].group &&
          x.inSubgroup
      );
      if (filterValue) {
        inputDataArray[i].key =
          inputDataArray[i].group + "_" + inputDataArray[i].key;
        inputDataArray[i].label = filterValue.label;
        inputDataArray[i].groupLabel = fullDataArray.find(
          (x) => x.subgroup === inputDataArray[i].group
        ).label;
        filterItems.push(inputDataArray[i]);
      } else {
        filterValue = fullDataArray.find(
          (x) => x.subgroup === inputDataArray[i].group
        );
        if (filterValue) {
          const subgroupItems = fullDataArray.filter(
            (t) => t.group === filterValue.subgroup
          );
          if (subgroupItems.length > 1) {
            inputDataArray[i].key = inputDataArray[i].group + "_-1";
            inputDataArray[i].label = filterValue.defaultSelectLabel;
            inputDataArray[i].groupLabel = fullDataArray.find(
              (x) => x.subgroup === inputDataArray[i].group
            ).label;
            filterItems.push(inputDataArray[i]);
          } else if (subgroupItems.length === 1) {
            const selectFilterItem = {
              key: subgroupItems[0].group + "_" + subgroupItems[0].key,
              group: subgroupItems[0].group,
              label: subgroupItems[0].label,
              groupLabel: fullDataArray.find(
                (x) => x.subgroup === subgroupItems[0].group
              ).label,
              inSubgroup: true,
            };
            filterItems.push(selectFilterItem);
          }
        }
      }
    }
  }
  return filterItems;
};

const minWidth = 170;
const filterItemPadding = 60;
const maxFilterItemWidth = 260;
const itemsContainerWidth = 40;
const sectionPaddings = 48;

class FilterInput extends React.Component {
  constructor(props) {
    super(props);

    const { selectedFilterData, getSortData, value } = props;
    const { sortDirection, sortId, inputValue } = selectedFilterData;
    const sortData = getSortData();

    const filterValues = selectedFilterData ? this.getDefaultFilterData() : [];

    this.state = {
      sortDirection: sortDirection === "desc" ? true : false,
      sortId:
        sortData.findIndex((x) => x.key === sortId) != -1
          ? sortId
          : sortData.length > 0
          ? sortData[0].key
          : "",
      searchText: inputValue || value,

      filterValues,
      openFilterItems: [],
      hiddenFilterItems: [],
      needUpdateFilter: false,
      asideView: false,
    };

    this.searchWrapper = React.createRef();
    this.filterWrapper = React.createRef();
    this.rectComboBoxRef = React.createRef();
  }

  componentDidMount() {
    if (this.state.filterValues.length > 0) this.updateFilter();
  }

  componentDidUpdate(prevProps, prevState) {
    const { selectedFilterData, sectionWidth, getSortData } = this.props;
    const { filterValues, searchText } = this.state;
    const { sortDirection, sortId, inputValue } = selectedFilterData;

    if (
      this.props.needForUpdate &&
      this.props.needForUpdate(prevProps, this.props)
    ) {
      let internalFilterData = convertToInternalData(
        this.props.getFilterData(),
        cloneObjectsArray(selectedFilterData.filterValues)
      );
      this.updateFilter(internalFilterData);
    }

    if (sectionWidth !== prevProps.sectionWidth) {
      this.updateFilter();
    }

    if (
      (!isEqual(selectedFilterData.filterValues, filterValues) ||
        inputValue !== searchText) &&
      sectionWidth !== prevProps.sectionWidth
    ) {
      const sortData = getSortData();
      const filterValues = this.getDefaultFilterData();
      this.setState({
        sortDirection: sortDirection === "desc" ? true : false,
        sortId:
          sortData.findIndex((x) => x.key === sortId) != -1
            ? sortId
            : sortData.length > 0
            ? sortData[0].key
            : "",
        filterValues: filterValues,
        searchText: selectedFilterData.inputValue || "",
      });
      this.updateFilter(filterValues);
    }

    if (
      !isEqual(
        prevProps.selectedFilterData.filterValues,
        selectedFilterData.filterValues
      ) &&
      selectedFilterData.filterValues &&
      (selectedFilterData.filterValues.length === 0 ||
        (selectedFilterData.filterValues.length === 1 &&
          selectedFilterData.filterValues[0].key === "null")) &&
      !selectedFilterData.inputValue
    ) {
      this.clearFilter();
    }
  }
  shouldComponentUpdate(nextProps, nextState) {
    const {
      selectedFilterData,
      value,
      id,
      isDisabled,
      size,
      placeholder,
      sectionWidth,
    } = this.props;

    if (
      !isEqual(selectedFilterData, nextProps.selectedFilterData) ||
      this.props.viewAs !== nextProps.viewAs ||
      this.props.widthProp !== nextProps.widthProp ||
      sectionWidth !== nextProps.sectionWidth
    ) {
      return true;
    }

    if (
      id != nextProps.id ||
      isDisabled != nextProps.isDisabled ||
      size != nextProps.size ||
      placeholder != nextProps.placeholder ||
      value != nextProps.value
    )
      return true;

    return !isEqual(this.state, nextState);
  }

  onChangeSortDirection = (key) => {
    this.onFilter(
      this.state.filterValues,
      this.state.sortId,
      key ? "desc" : "asc"
    );
    this.setState({ sortDirection: !!key });
  };
  onClickViewSelector = (item) => {
    const itemId = (item.target && item.target.dataset.for) || item;
    const viewAs = itemId.indexOf("row") === -1 ? "tile" : "row";
    this.props.onChangeViewAs(viewAs);
  };

  onClickSortItem = (key) => {
    this.setState({ sortId: key });
    this.onFilter(
      this.state.filterValues,
      key,
      this.state.sortDirection ? "desc" : "asc"
    );
  };
  onSortDirectionClick = () => {
    this.onFilter(
      this.state.filterValues,
      this.state.sortId,
      !this.state.sortDirection ? "desc" : "asc"
    );
    this.setState({ sortDirection: !this.state.sortDirection });
  };
  onSearchChanged = (value) => {
    this.setState({ searchText: value });
    this.onFilter(
      this.state.filterValues,
      this.state.sortId,
      this.state.sortDirection ? "desc" : "asc",
      value
    );
  };
  onSearch = (result) => {
    this.onFilter(
      result.filterValues,
      this.state.sortId,
      this.state.sortDirection ? "desc" : "asc"
    );
  };
  getDefaultFilterData = () => {
    const { getFilterData, selectedFilterData } = this.props;
    const filterData = getFilterData();
    const filterItems = [];
    const filterValues = cloneObjectsArray(selectedFilterData.filterValues);

    for (let item of filterValues) {
      const filterValue = filterData.find(
        (x) => x.key === item.key && x.group === item.group
      );

      if (!filterValue) {
        const isSelector = item.group.includes("filter-author");

        if (isSelector) {
          const typeSelector = item.key.includes("user")
            ? "user"
            : item.key.includes("group")
            ? "group"
            : null;
          const underlined = item.key.indexOf("_") !== -1;
          const key = underlined
            ? item.key.slice(0, item.key.indexOf("_"))
            : item.key;
          const filesFilterValue = filterData.find(
            (x) => x.key === key && x.group === item.group
          );

          if (filesFilterValue) {
            const convertedItem = {
              key: item.group + "_" + item.key,
              label: filesFilterValue.label,
              group: item.group,
              groupLabel: filesFilterValue.label,
              typeSelector,
              groupsCaption: filesFilterValue.groupsCaption,
              defaultOptionLabel: filesFilterValue.defaultOptionLabel,
              defaultOption: filesFilterValue.defaultOption,
              defaultSelectLabel: filesFilterValue.defaultSelectLabel,
              selectedItem: filesFilterValue.selectedItem,
            };
            filterItems.push(convertedItem);
          }
        }
      }

      let groupLabel = "";
      const groupFilterItem = filterData.find((x) => x.key === item.group);
      if (groupFilterItem) {
        groupLabel = groupFilterItem.label;
      } else {
        const subgroupFilterItem = filterData.find(
          (x) => x.subgroup === item.group
        );
        if (subgroupFilterItem) {
          groupLabel = subgroupFilterItem.label;
        }
      }

      if (filterValue) {
        item.key = item.group + "_" + item.key;
        item.label = filterValue.selectedItem
          ? filterValue.selectedItem.label
          : filterValue.label;
        item.groupLabel = groupLabel;
        filterItems.push(item);
      }
    }

    return filterItems;
  };
  getFilterData = () => {
    const d = this.props.getFilterData();
    const result = [];
    d.forEach((element) => {
      if (!element.inSubgroup) {
        element.onClick =
          !element.isSeparator && !element.isHeader && !element.disabled
            ? () => this.onClickFilterItem(element)
            : undefined;
        element.key =
          element.group != element.key
            ? element.group + "_" + element.key
            : element.key;
        if (element.subgroup != undefined) {
          if (d.findIndex((x) => x.group === element.subgroup) === -1)
            element.disabled = true;
        }
        result.push(element);
      }
    });
    return result;
  };
  clearFilter = () => {
    this.setState({
      searchText: "",
      filterValues: [],
      openFilterItems: [],
      hiddenFilterItems: [],
    });
    this.onFilter(
      [],
      this.state.sortId,
      this.state.sortDirection ? "desc" : "asc",
      ""
    );
  };

  getTextWidth = (text, font) => {
    var canvas =
      this.getTextWidth.canvas ||
      (this.getTextWidth.canvas = document.createElement("canvas"));
    var context = canvas.getContext("2d");
    context.font = font;
    var metrics = context.measureText(text);
    return metrics.width;
  };

  calcHiddenItemWidth = (item) => {
    if (!item) return;
    let label = this.getItemLabel(item);

    const itemWidth =
      this.getTextWidth(
        item.groupLabel + " " + label,
        "bolder 13px sans-serif"
      ) + filterItemPadding;
    return itemWidth;
  };

  addItems = (searchWidth) => {
    const { hiddenFilterItems } = this.state;
    if (hiddenFilterItems.length === 0) return 0;

    let newSearchWidth = searchWidth;
    let numberOfHiddenItems = hiddenFilterItems.length;

    for (let i = 0; i < hiddenFilterItems.length; i++) {
      let hiddenItemWidth = this.calcHiddenItemWidth(hiddenFilterItems[i]);

      if (hiddenItemWidth > maxFilterItemWidth)
        hiddenItemWidth = maxFilterItemWidth;
      newSearchWidth = newSearchWidth - hiddenItemWidth;
      if (newSearchWidth >= minWidth) {
        numberOfHiddenItems--;
      } else {
        break;
      }
    }

    return numberOfHiddenItems;
  };

  hideItems = (searchWidth, currentFilterItems) => {
    const { hiddenFilterItems } = this.state;
    let newSearchWidth = searchWidth;
    let numberOfHiddenItems = hiddenFilterItems.length;

    for (let i = currentFilterItems.length - 1; i >= 0; i--) {
      if (currentFilterItems[i].id === "styled-hide-filter") continue;
      const filterItemWidth = currentFilterItems[i].getBoundingClientRect()
        .width;
      newSearchWidth = newSearchWidth + filterItemWidth;
      numberOfHiddenItems++;
      if (numberOfHiddenItems === 1) newSearchWidth - itemsContainerWidth; // subtract the width of hidden block

      if (newSearchWidth > minWidth) break;
    }

    return numberOfHiddenItems;
  };

  calcHiddenItems = (searchWidth, currentFilterItems) => {
    const { hiddenFilterItems } = this.state;

    if (!searchWidth || currentFilterItems.length === 0)
      return hiddenFilterItems.length;
    const numberOfHiddenItems =
      searchWidth < minWidth
        ? this.hideItems(searchWidth, currentFilterItems)
        : this.addItems(searchWidth);

    return numberOfHiddenItems;
  };

  updateFilter = (inputFilterItems) => {
    const { sectionWidth } = this.props;
    const currentFilterItems = inputFilterItems
      ? cloneObjectsArray(inputFilterItems)
      : cloneObjectsArray(this.state.filterValues);
    const fullWidth = this.searchWrapper.current.getBoundingClientRect().width;
    const filterWidth = this.filterWrapper.current.getBoundingClientRect()
      .width;
    const comboBoxWidth = this.rectComboBoxRef.current.getBoundingClientRect()
      .width;

    const searchWidth = sectionWidth
      ? sectionWidth - filterWidth - comboBoxWidth - sectionPaddings
      : fullWidth - filterWidth;

    if (searchWidth) {
      const asideView = searchWidth && searchWidth < 350;

      this.setState({
        asideView,
      });
    }

    const filterArr = Array.from(
      Array.from(this.filterWrapper.current.children).find(
        (x) => x.id === "filter-items-container"
      ).children
    );
    const numberOfHiddenItems = this.calcHiddenItems(searchWidth, filterArr);
    if (searchWidth !== 0 && currentFilterItems.length > 0) {
      this.setState({
        openFilterItems: numberOfHiddenItems
          ? currentFilterItems.slice(
              0,
              currentFilterItems.length - numberOfHiddenItems
            )
          : currentFilterItems.slice(),
        hiddenFilterItems: numberOfHiddenItems
          ? currentFilterItems.slice(-numberOfHiddenItems)
          : [],
      });
    }
  };

  getItemLabel = (item) => {
    let label = "";

    if (item.selectedItem) {
      label = item.selectedItem.label
        ? item.selectedItem.label
        : item.defaultSelectLabel;
    } else {
      label = item.label;
    }
    return label;
  };

  onDeleteFilterItem = (key) => {
    const currentFilterItems = this.state.filterValues.slice();
    const indexFilterItem = currentFilterItems.findIndex((x) => x.key === key);
    if (indexFilterItem != -1) {
      currentFilterItems.splice(indexFilterItem, 1);
    }
    this.setState({
      filterValues: currentFilterItems,
      openFilterItems: currentFilterItems,
      hiddenFilterItems: [],
    });
    let filterValues = cloneObjectsArray(currentFilterItems);
    filterValues = filterValues.map(function (item) {
      item.key = item.key.replace(item.group + "_", "");
      return item;
    });
    this.onFilter(
      filterValues.filter((item) => item.key != "-1"),
      this.state.sortId,
      this.state.sortDirection ? "desc" : "asc"
    );
  };
  onFilter = (filterValues, sortId, sortDirection, searchText) => {
    let cloneFilterValues = cloneObjectsArray(filterValues);
    cloneFilterValues = cloneFilterValues.map(function (item) {
      item.key = item.key.replace(item.group + "_", "");
      return item;
    });
    this.props.onFilter({
      inputValue: searchText != undefined ? searchText : this.state.searchText,
      filterValues: cloneFilterValues.filter((item) => item.key != "-1"),
      sortId: sortId,
      sortDirection: sortDirection,
    });
  };
  onChangeFilter = (result) => {
    this.setState({
      searchText: result.inputValue,
      filterValues: result.filterValues,
    });
    this.onFilter(
      result.filterValues,
      this.state.sortId,
      this.state.sortDirection ? "desc" : "asc",
      result.inputValue
    );
  };
  onFilterRender = () => {
    this.setState({
      needUpdateFilter: false,
    });

    this.updateFilter();
  };
  onClickFilterItem = (event, filterItem) => {
    const currentFilterItems = cloneObjectsArray(this.state.filterValues);

    this.setState({
      needUpdateFilter: true,
    });

    if (filterItem.isSelector) {
      const indexFilterItem = currentFilterItems.findIndex(
        (x) => x.group === filterItem.group
      );
      if (indexFilterItem != -1) {
        currentFilterItems.splice(indexFilterItem, 1);
      }
      const typeSelector = filterItem.key.includes("user")
        ? "user"
        : filterItem.key.includes("group")
        ? "group"
        : null;
      const itemId =
        filterItem.key.indexOf("_") !== filterItem.key.lastIndexOf("_")
          ? filterItem.key.slice(0, filterItem.key.lastIndexOf("_"))
          : filterItem.key;
      const itemKey =
        filterItem.selectedItem &&
        filterItem.selectedItem.key &&
        filterItem.typeSelector === typeSelector
          ? itemId + "_" + filterItem.selectedItem.key
          : filterItem.key + "_-1";
      const selectedItem =
        filterItem.typeSelector === typeSelector ? filterItem.selectedItem : {};
      const selectFilterItem = {
        key: itemKey,
        group: filterItem.group,
        label: filterItem.label,
        groupLabel: filterItem.label,
        typeSelector,
        defaultOption: filterItem.defaultOption,
        groupsCaption: filterItem.groupsCaption,
        defaultOptionLabel: filterItem.defaultOptionLabel,
        defaultSelectLabel: filterItem.defaultSelectLabel,
        selectedItem,
      };
      currentFilterItems.push(selectFilterItem);
      this.setState({
        filterValues: currentFilterItems,
        openFilterItems: currentFilterItems,
        hiddenFilterItems: [],
      });

      if (selectFilterItem.selectedItem.key) {
        const clone = cloneObjectsArray(
          currentFilterItems.filter((item) => item.key != "-1")
        );
        clone.map(function (item) {
          item.key = item.key.replace(item.group + "_", "");
          return item;
        });

        this.onFilter(
          clone.filter((item) => item.key != "-1"),
          this.state.sortId,
          this.state.sortDirection ? "desc" : "asc"
        );
      }

      return;
    }

    if (filterItem.subgroup) {
      const indexFilterItem = currentFilterItems.findIndex(
        (x) => x.group === filterItem.subgroup
      );
      if (indexFilterItem != -1) {
        currentFilterItems.splice(indexFilterItem, 1);
      }
      const subgroupItems = this.props
        .getFilterData()
        .filter((t) => t.group === filterItem.subgroup);
      if (subgroupItems.length > 1) {
        const selectFilterItem = {
          key: filterItem.subgroup + "_-1",
          group: filterItem.subgroup,
          label: filterItem.defaultSelectLabel,
          groupLabel: filterItem.label,
          inSubgroup: true,
        };
        if (indexFilterItem != -1)
          currentFilterItems.splice(indexFilterItem, 0, selectFilterItem);
        else currentFilterItems.push(selectFilterItem);
        this.setState({
          filterValues: currentFilterItems,
          openFilterItems: currentFilterItems,
          hiddenFilterItems: [],
        });
      } else if (subgroupItems.length === 1) {
        const selectFilterItem = {
          key: subgroupItems[0].group + "_" + subgroupItems[0].key,
          group: subgroupItems[0].group,
          label: subgroupItems[0].label,
          groupLabel: this.props
            .getFilterData()
            .find((x) => x.subgroup === subgroupItems[0].group).label,
          inSubgroup: true,
        };
        if (indexFilterItem != -1)
          currentFilterItems.splice(indexFilterItem, 0, selectFilterItem);
        else currentFilterItems.push(selectFilterItem);

        const clone = cloneObjectsArray(
          currentFilterItems.filter((item) => item.key != "-1")
        );
        clone.map(function (item) {
          item.key = item.key.replace(item.group + "_", "");
          return item;
        });
        this.onFilter(
          clone.filter((item) => item.key != "-1"),
          this.state.sortId,
          this.state.sortDirection ? "desc" : "asc"
        );
        this.setState({
          filterValues: currentFilterItems,
          openFilterItems: currentFilterItems,
          hiddenFilterItems: [],
        });
      }
    } else {
      const filterItems = this.getFilterData();

      const indexFilterItem = currentFilterItems.findIndex(
        (x) => x.group === filterItem.group
      );
      if (indexFilterItem != -1) {
        currentFilterItems.splice(indexFilterItem, 1);
      }

      const selectFilterItem = {
        key: filterItem.key,
        group: filterItem.group,
        label: filterItem.label,
        groupLabel: filterItem.inSubgroup
          ? filterItems.find((x) => x.subgroup === filterItem.group).label
          : filterItems.find((x) => x.key === filterItem.group).label,
      };
      if (indexFilterItem != -1)
        currentFilterItems.splice(indexFilterItem, 0, selectFilterItem);
      else currentFilterItems.push(selectFilterItem);
      this.setState({
        filterValues: currentFilterItems,
        openFilterItems: currentFilterItems,
        hiddenFilterItems: [],
      });

      const clone = cloneObjectsArray(
        currentFilterItems.filter((item) => item.key != "-1")
      );
      clone.map(function (item) {
        item.key = item.key.replace(item.group + "_", "");
        return item;
      });
      this.onFilter(
        clone.filter((item) => item.key != "-1"),
        this.state.sortId,
        this.state.sortDirection ? "desc" : "asc"
      );
    }
  };

  render() {
    /* eslint-disable react/prop-types */
    const {
      className,
      id,
      style,
      size,
      isDisabled,
      scale,
      getFilterData,
      placeholder,
      getSortData,
      directionAscLabel,
      directionDescLabel,
      filterColumnCount,
      viewAs,
      contextMenuHeader,
      isMobile,
      sectionWidth,
    } = this.props;
    /* eslint-enable react/prop-types */

    const {
      searchText,
      filterValues,
      openFilterItems,
      hiddenFilterItems,
      sortId,
      sortDirection,
      asideView,
      needUpdateFilter,
    } = this.state;

    const smallSectionWidth = sectionWidth ? sectionWidth <= 500 : false;
    const isAllItemsHide =
      openFilterItems.length === 0 && hiddenFilterItems.length > 0
        ? true
        : false;

    let iconSize = 30;
    switch (size) {
      case "base":
        iconSize = 32;
        break;
      case "middle":
      case "big":
      case "huge":
        iconSize = 41;
        break;
      default:
        break;
    }
    return (
      <StyledFilterInput
        smallSectionWidth={smallSectionWidth}
        isMobile={isMobile}
        viewAs={viewAs}
        className={className}
        id={id}
        style={style}
        isAllItemsHide={isAllItemsHide}
      >
        <div className="styled-search-input test" ref={this.searchWrapper}>
          <SearchInput
            id={id}
            isDisabled={isDisabled}
            size={size}
            scale={scale}
            isNeedFilter={true}
            getFilterData={getFilterData}
            placeholder={placeholder}
            onSearchClick={this.onSearch}
            onChangeFilter={this.onChangeFilter}
            value={searchText}
            selectedFilterData={filterValues}
            showClearButton={filterValues.length > 0}
            onClearSearch={this.clearFilter}
            onChange={this.onSearchChanged}
          >
            <div className="styled-filter-block" ref={this.filterWrapper}>
              <FilterBlock
                contextMenuHeader={contextMenuHeader}
                openFilterItems={openFilterItems}
                hiddenFilterItems={hiddenFilterItems}
                iconSize={iconSize}
                getFilterData={getFilterData}
                onClickFilterItem={this.onClickFilterItem}
                onDeleteFilterItem={this.onDeleteFilterItem}
                onFilterRender={this.onFilterRender}
                isDisabled={isDisabled}
                columnCount={filterColumnCount}
                needUpdateFilter={needUpdateFilter}
                asideView={asideView}
              />
            </div>
          </SearchInput>
        </div>
        <div ref={this.rectComboBoxRef}>
          <SortComboBox
            options={getSortData()}
            isDisabled={isDisabled}
            onChangeSortId={this.onClickSortItem}
            onChangeView={this.onClickViewSelector}
            onChangeSortDirection={this.onChangeSortDirection}
            selectedOption={
              getSortData().length > 0
                ? getSortData().find((x) => x.key === sortId)
                : {}
            }
            onButtonClick={this.onSortDirectionClick}
            viewAs={viewAs}
            sortDirection={+sortDirection}
            directionAscLabel={directionAscLabel}
            directionDescLabel={directionDescLabel}
          />
        </div>
        {viewAs && (
          <ViewSelector
            isDisabled={isDisabled}
            onClickViewSelector={this.onClickViewSelector}
            viewAs={viewAs}
          />
        )}
      </StyledFilterInput>
    );
  }
}

FilterInput.propTypes = {
  size: PropTypes.oneOf(["base", "middle", "big", "huge"]),
  autoRefresh: PropTypes.bool,
  selectedFilterData: PropTypes.object,
  directionAscLabel: PropTypes.string,
  directionDescLabel: PropTypes.string,
  viewAs: PropTypes.bool, // TODO: include viewSelector after adding method getThumbnail - PropTypes.string
  className: PropTypes.string,
  id: PropTypes.string,
  style: PropTypes.oneOfType([PropTypes.object, PropTypes.array]),
  needForUpdate: PropTypes.oneOfType([PropTypes.bool, PropTypes.func]),
  filterColumnCount: PropTypes.number,
  onChangeViewAs: PropTypes.func,
  contextMenuHeader: PropTypes.string,
  sectionWidth: PropTypes.number,
  getSortData: PropTypes.func,
  value: PropTypes.string,
};

FilterInput.defaultProps = {
  autoRefresh: true,
  selectedFilterData: {
    sortDirection: false,
    sortId: "",
    filterValues: [],
    searchText: "",
  },
  size: "base",
  needForUpdate: false,
  directionAscLabel: "A-Z",
  directionDescLabel: "Z-A",
};

export default FilterInput;
