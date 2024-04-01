using System;
using System.Text.Json.Serialization;

namespace StreamSentinel.Entities.Geometric
{
    public class ImageBasedGeometric
    {
        private int _imageWidth;
        private int _imageHeight;
        private bool _initialized;
        
        [JsonIgnore]
        public int ImageWidth
        {
            get
            {
                CheckImageSizeInitialized();
                return _imageWidth;
            }
        }

        [JsonIgnore]
        public int ImageHeight
        {
            get
            {
                CheckImageSizeInitialized();
                return _imageHeight;
            }
        }

        protected ImageBasedGeometric()
        {
            _initialized = false;
        }

        protected ImageBasedGeometric(int imageWidth, int imageHeight)
        {
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;
            _initialized = true;
        }

        public virtual void SetImageSize(int width, int height)
        {
            if ((width < 0) && (height < 0))
            {
                throw new ArgumentException("image width or height not correct.");
            }
            
            _imageWidth = width;
            _imageHeight = height;
            _initialized = true;
        }

        protected bool IsInitialized()
        {
            return _initialized;
        }
        
        protected void CheckImageSizeInitialized()
        {
            if (!_initialized)
            {
                throw new ArgumentException("image width or height not set");
            }
        }
    }
}