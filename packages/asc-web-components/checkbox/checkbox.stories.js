import React from "react";

import Checkbox from "./";

export default {
  title: "Components/Checkbox",
  component: Checkbox,
  parameters: {
    docs: {
      description: { component: "Custom checkbox input" },
      source: {
        code: `
        import Checkbox from "@appserver/components/checkbox";

<Checkbox
  id="id"
  name="name"
  value="value"
  label="label"
  isChecked={false}
  isIndeterminate={false}
  isDisabled={false}
  onChange={() => {}}
/>
        `,
      },
    },
  },
  argTypes: {
    className: { description: "Accepts class" },
    id: { description: "Used as HTML id property" },
    isChecked: {
      description: "The checked property sets the checked state of a checkbox",
    },
    isDisabled: { description: "Disables the Checkbox input " },
    isIndeterminate: {
      description:
        "If true, this state is shown as a rectangle in the checkbox",
    },
    label: { description: "Label of the input" },
    name: { description: "Used as HTML `name` property" },
    onChange: {
      description: "Will be triggered whenever an CheckboxInput is clicked ",
      action: "onChange",
    },
    style: { description: "Accepts css style " },
    value: { description: "Value of the input" },
    title: { description: "Title " },
    truncate: { description: "Disables word wrapping" },
  },
};

class Wrapper extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isChecked: false,
    };
  }

  onChange = (e) => {
    this.props.onChange(e);
    this.setState({ isChecked: !this.state.isChecked });
  };

  render() {
    return (
      <Checkbox
        {...this.props}
        isChecked={this.props.isChecked || this.state.isChecked}
        onChange={this.onChange}
      />
    );
  }
}
const Template = (args) => {
  return <Wrapper {...args} />;
};

const AllCheckboxesTemplate = (args) => {
  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat( auto-fill, minmax(120px, 1fr) )",
        gridGap: "16px",
        alignItems: "center",
      }}
    >
      <Checkbox />
      <Checkbox isChecked={true} />
      <Checkbox isDisabled={true} />
      <Checkbox isIndeterminate={true} />
      <Checkbox label="Some label" />
    </div>
  );
};

export const Default = Template.bind({});
export const AllCheckboxStates = AllCheckboxesTemplate.bind({});
/*import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import { BooleanValue } from "react-values";
import { withKnobs, boolean, text } from "@storybook/addon-knobs/react";
import withReadme from "storybook-readme/with-readme";
import Readme from "./README.md";
import Checkbox from ".";
import Section from "../../../.storybook/decorators/section";

storiesOf("Components|Input", module)
  .addDecorator(withKnobs)
  .addDecorator(withReadme(Readme))
  .add("checkbox", () => (
    <Section>
      <BooleanValue>
        {({ value, toggle }) => (
          <Checkbox
            id={text("id", "id")}
            name={text("name", "name")}
            value={text("value", "value")}
            label={text("label", "label")}
            isChecked={value}
            isIndeterminate={boolean("isIndeterminate", false)}
            isDisabled={boolean("isDisabled", false)}
            onChange={(e) => {
              action("onChange")(e);
              toggle(e.target.checked);
            }}
          />
        )}
      </BooleanValue>
    </Section>
  ));*/
