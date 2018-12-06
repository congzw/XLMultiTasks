namespace XLMultiTasks.Common
{
    /// <summary>
    /// 自定义返回结果
    /// </summary>
    public class MessageResult
    {
        public MessageResult()
        {
            Success = false;
        }

        public MessageResult(bool success, string message, object data = null)
        {
            Success = success;
            _message = message;
            _data = data;
        }

        private string _message = string.Empty;
        private object _data = null;

        public bool Success { get; set; }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        public virtual object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public static MessageResult CreateByCrudFlag(object flag, string message = "")
        {
            int flagNum = (int)flag;
            MessageResult result = new MessageResult();
            if (flagNum > 0)
            {
                result.Success = true;
                result.Message = "保存成功";
            }
            else
            {
                result.Success = false;
                result.Message = "保存失败";
            }

            if (!string.IsNullOrEmpty(message))
            {
                result.Message = message;
            }
            return result;
        }

        public static MessageResult ValidateResult(bool validateSuccess = false, string successMessage = "验证通过", string failMessage = "验证失败")
        {
            MessageResult vr = new MessageResult();
            vr.Message = validateSuccess ? successMessage : failMessage;
            vr.Success = validateSuccess;
            return vr;
        }

        public static MessageResult CreateMessageResult(bool success, string successMessage = "成功", string failMessage = "失败")
        {
            MessageResult mr = new MessageResult();
            mr.Message = success ? successMessage : failMessage;
            mr.Success = success;
            return mr;
        }

        /// <summary>
        /// 保证不是默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="displayName"></param>
        public static MessageResult MakeSureIsNotDefault<T>(T instance, string displayName = null)
        {
            var validateResult = ValidateResult();
            var value = default(T);
            bool isEqual = Equals(instance, value);
            if (!isEqual)
            {
                validateResult.Success = true;
                validateResult.Message = "OK";
                return validateResult;
            }

            string exMessage = string.Format("值不能为:{0}", instance);
            exMessage = exMessage == "值不能为:" ? "值不能为:null" : exMessage;
            validateResult.Message = exMessage;
            return validateResult;
        }

        public override string ToString()
        {
            return string.Format("Success:{0}, Message:{1}, Data:{2}", Success, Message, Data);
        }

        #region for fluent style call

        /// <summary>
        /// fluent style set success
        /// </summary>
        /// <param name="success"></param>
        /// <returns></returns>
        public MessageResult WithSuccess(bool success)
        {
            this.Success = success;
            return this;
        }

        /// <summary>
        /// fluent style set message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public MessageResult WithMessage(string message)
        {
            this.Message = message;
            return this;
        }

        /// <summary>
        /// fluent style set data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MessageResult WithData(object data)
        {
            this.Data = data;
            return this;
        }

        /// <summary>
        /// 自动补齐信息  prefix + (success ? "成功" : "失败")
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="autoAppendSuccessFail"></param>
        /// <returns></returns>
        public MessageResult WithSuccessMessage(bool success, string message, bool autoAppendSuccessFail = false)
        {
            this.Success = success;
            if (autoAppendSuccessFail)
            {
                this.Message = message + (success ? "成功" : "失败");
            }
            else
            {
                this.Message = message;
            }
            return this;
        }

        #endregion

    }
}
