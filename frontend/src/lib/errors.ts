export enum ErrorType {
	ThreadIdValidation = 'threadIdValidation'
}

export class DomainError extends Error {
	type: ErrorType
	constructor(errorType: ErrorType) {
		super()
		this.type = errorType
	}
}
