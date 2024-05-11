
class zForm {
  fields = {};
  errors = {};
  isValid = false;
  errorSummary = '';
  zSchema = null;
  zSchemaKeys = null;

  constructor(schema) {
    this.zSchema = schema;

    this.zSchemaKeys = Object.keys(this.zSchema.shape);

    this.zSchemaKeys.forEach(name => {
      this.fields[name] = '';
      this.errors[name] = '';
    });
  }

  clearErrors() {
    this.errorSummary = '';
    this.zSchemaKeys.forEach(name => {
      this.errors[name] = '';
    });
  }

  reset() {
    this.clearErrors();

    this.zSchemaKeys.forEach(name => {
      this.fields[name] = '';
    });
  }

  validate(showMessage = false) {
    this.clearErrors();
    this.isValid = true;

    this.zSchemaKeys.forEach(name => {
      try {
        this.zSchema.shape[name].parse(this.fields[name]);
      } catch (error) {
        if (showMessage) {
          this.errors[name] = error.issues[0].message;
        } else {
          this.errors[name] = '*';
        }
        this.isValid = false;
      }
    });
  }
};

export default zForm;